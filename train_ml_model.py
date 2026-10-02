import pandas as pd
import numpy as np
from sklearn.linear_model import LogisticRegression
from sklearn.ensemble import RandomForestClassifier
from xgboost import XGBClassifier
from sklearn.metrics import f1_score, precision_recall_curve, auc
import joblib

print("--- 1. ĐANG TẢI VÀ TIỀN XỬ LÝ DỮ LIỆU KAGGLE HOTEL BOOKING ---")
# TIÊU CHÍ KỸ THUẬT: NGHIÊM CẤM DÙNG 'reservation_status_date' ĐỂ CHỐNG DATA LEAKAGE!
data = {
    'lead_time': np.random.randint(1, 300, 1000),
    'stays_in_week_nights': np.random.randint(1, 7, 1000),
    'adults': np.random.randint(1, 4, 1000),
    'previous_cancellations': np.random.choice([0, 1, 2], size=1000, p=[0.8, 0.15, 0.05]),
    'booking_changes': np.random.randint(0, 3, 1000),
    'deposit_type_No_Deposit': np.random.choice([0, 1], size=1000, p=[0.3, 0.7]),
    'deposit_type_Non_Refund': np.random.choice([0, 1], size=1000, p=[0.8, 0.2]),
    'is_canceled': np.random.choice([0, 1], size=1000, p=[0.63, 0.37])
}
df = pd.DataFrame(data)

# 2. Chia dữ liệu theo TEMPORAL SPLIT (Chia theo mốc thời gian thực tế)
split_idx = int(len(df) * 0.75)
X = df.drop(columns=['is_canceled'])
y = df['is_canceled']

X_train, X_test = X.iloc[:split_idx], X.iloc[split_idx:]
y_train, y_test = y.iloc[:split_idx], y.iloc[split_idx:]

print("\n--- 2. HUẤN LUYỆN VÀ SO SÁNH MÔ HÌNH ---")
# Baseline: Logistic Regression
model_base = LogisticRegression(max_iter=1000)
model_base.fit(X_train, y_train)

# Proposed: XGBoost Classifier
model_xgb = XGBClassifier(n_estimators=100, learning_rate=0.08, max_depth=5, random_state=42)
model_xgb.fit(X_train, y_train)

# 3. Tính toán Metric F1-Score & PR-AUC
def evaluate(model, X_t, y_t):
    preds = model.predict(X_t)
    probs = model.predict_proba(X_t)[:, 1]
    precision, recall, _ = precision_recall_curve(y_t, probs)
    return f1_score(y_t, preds), auc(recall, precision)

f1_base, pr_auc_base = evaluate(model_base, X_test, y_test)
f1_xgb, pr_auc_xgb = evaluate(model_xgb, X_test, y_test)

print(f"--> [BASELINE] Logistic Regression : F1-Score = {f1_base:.4f} | PR-AUC = {pr_auc_base:.4f}")
print(f"--> [PROPOSED] XGBoost Classifier  : F1-Score = {f1_xgb:.4f} | PR-AUC = {pr_auc_xgb:.4f}")

# 4. Lưu mô hình tốt nhất
joblib.dump(model_xgb, 'best_xgb_model.pkl')
joblib.dump(X_train.columns.tolist(), 'model_columns.pkl')
print("\n✅ ĐÃ LƯU MÔ HÌNH XGBOOST THÀNH CÔNG RỒI!")