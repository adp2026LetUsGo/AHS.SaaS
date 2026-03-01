import numpy as np
import pandas as pd
from sklearn.ensemble import RandomForestClassifier
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import FloatTensorType

# 1. Generate Synthetic Training Data
# Features: Route (0-9), Carrier (0-4), Packaging (0-2), Weather (0-3)
def generate_data(n_samples=2000):
    np.random.seed(42)
    route = np.random.randint(0, 10, n_samples)
    carrier = np.random.randint(0, 5, n_samples)
    packaging = np.random.randint(0, 3, n_samples)
    weather = np.random.randint(0, 4, n_samples)
    
    # Simple logic for ground truth risk
    # Certain combinations increase risk
    # e.g., Weather > 2 (Extreme) and long routes or specific carriers
    risk_score = (weather * 15) + (route * 2) + (carrier * 3) + (packaging * 5)
    
    # Probability conversion
    prob = 1 / (1 + np.exp(-(risk_score - 35) / 5))
    is_excursion = (np.random.random(n_samples) < prob).astype(int)
    
    data = pd.DataFrame({
        'route': route.astype(np.float32),
        'carrier': carrier.astype(np.float32),
        'packaging': packaging.astype(np.float32),
        'weather': weather.astype(np.float32)
    })
    return data, is_excursion

print("Generating synthetic data for 4 features...")
X, y = generate_data()

# 2. Train Model
print("Training RandomForest model...")
model = RandomForestClassifier(n_estimators=100, max_depth=5, random_state=42)
model.fit(X, y)

# 3. Convert to ONNX
print("Converting model to ONNX...")
# Feature names in the model should match the inference input names if possible,
# but 'float_input' is standard for single tensor.
initial_type = [('float_input', FloatTensorType([None, 4]))]
onnx_model = convert_sklearn(model, initial_types=initial_type, 
                             options={type(model): {'zipmap': False}}) # ZipMap False for simpler C# access

# 4. Save ONNX File
onnx_filename = "excursion_risk_v1.onnx"
with open(onnx_filename, "wb") as f:
    f.write(onnx_model.SerializeToString())

print(f"Model saved to {onnx_filename}")
