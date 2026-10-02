from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import joblib
import pandas as pd
import shap

app = FastAPI(title="Hotel PMS DSS & AI Engine")

# BẬT QUYỀN KẾT NỐI CORS CHO TRÌNH DUYỆT C# GỌI API
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Load model XGBoost
model = joblib.load('best_xgb_model.pkl')
columns = joblib.load('model_columns.pkl')
explainer = shap.TreeExplainer(model)

class BookingPayload(BaseModel):
    lead_time: int = 45
    stays_in_week_nights: int = 2
    adults: int = 2
    previous_cancellations: int = 0
    booking_changes: int = 0
    deposit_type_No_Deposit: int = 1
    deposit_type_Non_Refund: int = 0

@app.post("/api/dss/predict")
def predict_cancellation(payload: BookingPayload):
    input_df = pd.DataFrame([payload.dict()])
    input_df = input_df.reindex(columns=columns, fill_value=0)
    
    # 1. Xác suất hủy phòng
    prob = float(model.predict_proba(input_df)[0][1])
    
    # 2. SHAP Explainer
    shap_vals = explainer.shap_values(input_df)[0]
    top_shap = dict(sorted(zip(columns, shap_vals), key=lambda x: abs(x[1]), reverse=True)[:3])
    
    # 3. Luật Hệ hỗ trợ quyết định (DSS Rules)
    risk_level = "CAO" if prob >= 0.55 else ("TRUNG BÌNH" if prob >= 0.35 else "THẤP")
    
    recommendation = ""
    if risk_level == "CAO":
        recommendation = "Cảnh báo rủi ro Hủy phòng CAO! Đề xuất DSS: Yêu cầu khách đặt cọc 30% hoặc bật chính sách Overbooking bán bù 1 phòng."
    elif risk_level == "TRUNG BÌNH":
        recommendation = "Rủi ro trung bình. Đề xuất DSS: Gửi Zalo/SMS nhắc nhở xác nhận trước 48h."
    else:
        recommendation = "An toàn. Giữ phòng mặc định."

    return {
        "cancel_probability": round(prob, 4),
        "risk_level": risk_level,
        "dss_recommendation": recommendation,
        "top_shap_factors": top_shap
    }