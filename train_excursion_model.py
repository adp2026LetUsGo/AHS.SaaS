import numpy as np
import pandas as pd
from sklearn.ensemble import RandomForestClassifier
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import FloatTensorType

# 1. Generate Synthetic Training Data
# Features: RouteId (hash), Carrier (hash), TransitTimeHrs, ExternalTempAvg, PackagingType (hash), DelayFlag
def generate_data(n_samples=1000):
    np.random.seed(42)
    transit_time = np.random.randint(12, 120, n_samples)
    external_temp = np.random.normal(20, 10, n_samples)
    delay_flag = np.random.choice([0, 1], n_samples, p=[0.8, 0.2])
    
    # Simple logic for ground truth risk
    # Risk increases with temp, time, and delays
    risk_score = (external_temp * 0.4) + (transit_time * 0.1) + (delay_flag * 20)
    risk_labels = (risk_score > 40).astype(int)
    
    data = pd.DataFrame({
        'TransitTimeHrs': transit_time.astype(np.float32),
        'ExternalTempAvg': external_temp.astype(np.float32),
        'DelayFlag': delay_flag.astype(np.float32)
    })
    return data, risk_labels

print("Generating synthetic data...")
X, y = generate_data()

# 2. Train Model
print("Training RandomForest model...")
model = RandomForestClassifier(n_estimators=100, max_depth=5, random_state=42)
model.fit(X, y)

# 3. Convert to ONNX
print("Converting model to ONNX...")
initial_type = [('float_input', FloatTensorType([None, 3]))]
onnx_model = convert_sklearn(model, initial_types=initial_type)

# 4. Save ONNX File
onnx_filename = "excursion_risk_v1.onnx"
with open(onnx_filename, "wb") as f:
    f.write(onnx_model.SerializeToString())

print(f"Model saved to {onnx_filename}")
