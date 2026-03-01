import numpy as np
import pandas as pd
from sklearn.ensemble import RandomForestClassifier
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import FloatTensorType

# MISSION: MODEL RE-CALIBRATION (4 FEATURES)
# Features: RouteId, Carrier, PackagingType, Weather
def generate_data(n_samples=2000):
    np.random.seed(42)
    route = np.random.randint(0, 10, n_samples)
    carrier = np.random.randint(0, 5, n_samples)
    packaging = np.random.randint(0, 3, n_samples)
    weather = np.random.randint(0, 4, n_samples)
    
    # Logic for risk probability
    risk_score = (weather * 15) + (route * 2) + (carrier * 3) + (packaging * 5)
    prob = 1 / (1 + np.exp(-(risk_score - 35) / 5))
    is_excursion = (np.random.random(n_samples) < prob).astype(int)
    
    data = pd.DataFrame({
        'RouteId': route.astype(np.float32),
        'Carrier': carrier.astype(np.float32),
        'PackagingType': packaging.astype(np.float32),
        'Weather': weather.astype(np.float32)
    })
    return data, is_excursion

print("Starting Model Re-calibration (4 Features)...")
X, y = generate_data()

print("Training RandomForestClassifier...")
model = RandomForestClassifier(n_estimators=100, max_depth=5, random_state=42)
model.fit(X, y)

print("Exporting to ONNX with input shape [1, 4]...")
# Feature names in the model should match the inference input names if possible.
# 'float_input' matches the C# code expectation.
initial_type = [('float_input', FloatTensorType([None, 4]))]
onnx_model = convert_sklearn(model, initial_types=initial_type, 
                             target_opset=21,
                             options={type(model): {'zipmap': False}})

onnx_filename = "excursion_risk_v1.onnx"
with open(onnx_filename, "wb") as f:
    f.write(onnx_model.SerializeToString())

print(f"Model saved to {onnx_filename} with 4 features.")
