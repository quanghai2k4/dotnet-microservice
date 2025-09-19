# .NET Microservice with Payment Simulation

Dự án microservice .NET với tính năng mô phỏng thanh toán, sử dụng RabbitMQ 4, Docker và NGINX API Gateway.

## Cấu trúc dự án

```
Microservice/
├── Services/
│   ├── UserService/          # Microservice quản lý người dùng
│   └── OrderService/         # Microservice quản lý đơn hàng & thanh toán
├── Common/
│   └── RabbitMQ.Client/      # Thư viện RabbitMQ chung
├── Gateway/
│   └── nginx/                # NGINX API Gateway
├── WebClient/                # Web UI cho quản lý hệ thống
└── docker-compose.yml        # Docker orchestration
```

## Công nghệ sử dụng

- **.NET 8**: Framework chính
- **RabbitMQ 4 Management**: Message broker với UI quản lý
- **Docker & Docker Compose**: Container orchestration
- **NGINX**: API Gateway và Load Balancer  
- **Entity Framework Core**: ORM (In-Memory Database)
- **Swagger/OpenAPI**: API Documentation
- **HTML/CSS/JavaScript**: Web Client UI

## Tính năng chính

### 🚀 Payment Simulation System
- **Mô phỏng thanh toán thực tế** với độ trễ 500-2000ms
- **Tỷ lệ thất bại ngẫu nhiên** 5% để mô phỏng lỗi thanh toán
- **Hỗ trợ nhiều phương thức thanh toán**: CreditCard, DebitCard, PayPal, BankTransfer
- **Transaction ID tự động** cho mỗi giao dịch thành công
- **Payment status tracking**: Unpaid → Processing → Paid/Failed

### 🌐 Web Management Interface
- **Responsive Web UI** để quản lý users và orders
- **Real-time payment processing** với visual feedback
- **Payment status indicators** với màu sắc trực quan
- **AJAX-based interactions** không cần reload trang

## Khởi chạy dự án

### Yêu cầu
- Docker Desktop
- .NET 8 SDK (để development)

### Chạy toàn bộ hệ thống
```bash
docker-compose up --build -d
```

### Truy cập ứng dụng
- **Web Application**: http://localhost:8080 
- **API Gateway**: http://localhost:8080/api
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

### Chạy từng service riêng lẻ
```bash
# Build và chạy từng service
docker-compose up --build userservice
docker-compose up --build orderservice
docker-compose up --build nginx-gateway
docker-compose up --build rabbitmq
```

## Endpoints API

### Thông qua NGINX Gateway (Port 8080)
- **Web Application**: http://localhost:8080 
- **User Service**: http://localhost:8080/api/users
- **Order Service**: http://localhost:8080/api/orders
- **Payment Processing**: http://localhost:8080/api/orders/{id}/payment

### Direct Service Access
- **User Service**: http://localhost:5001
- **Order Service**: http://localhost:5002  
- **Web Client**: Served via nginx gateway
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

## API Examples

### User Service
```bash
# Tạo user mới
curl -X POST http://localhost:8080/api/users \
  -H "Content-Type: application/json" \
  -d '{"name": "John Doe", "email": "john@example.com"}'

# Lấy danh sách users
curl http://localhost:8080/api/users

# Lấy user theo ID
curl http://localhost:8080/api/users/1
```

### Order Service
```bash
# Tạo order mới
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{"userId": 1, "productName": "Laptop", "quantity": 1, "price": 1000.00}'

# Lấy danh sách orders
curl http://localhost:8080/api/orders

# Lấy orders theo userId
curl http://localhost:8080/api/orders/user/1
```

### Payment Service
```bash
# Xử lý thanh toán cho order
curl -X POST http://localhost:8080/api/orders/1/payment \
  -H "Content-Type: application/json" \
  -d '{"paymentMethod": "CreditCard", "amount": 1000.00}'

# Kiểm tra trạng thái thanh toán
curl http://localhost:8080/api/orders/1
```

## 💳 Payment Simulation Features

### Supported Payment Methods
- **CreditCard**: Thẻ tín dụng
- **DebitCard**: Thẻ ghi nợ  
- **PayPal**: Ví điện tử PayPal
- **BankTransfer**: Chuyển khoản ngân hàng

### Payment Flow
1. **Create Order** → Status: "Pending", PaymentStatus: "Unpaid"
2. **Process Payment** → PaymentStatus: "Processing" (500-2000ms delay)
3. **Payment Result**:
   - ✅ **Success (95%)**: PaymentStatus: "Paid", Status: "Paid", TransactionId generated
   - ❌ **Failed (5%)**: PaymentStatus: "Failed", Status: "Pending"

### Payment Response Example
```json
{
  "isSuccess": true,
  "transactionId": "TXN_20231219143052_7489",
  "message": "Payment processed successfully"
}
```

## Message Queue Events

### User Created Event
- **Exchange**: `user.events`
- **Routing Key**: `user.created`
- **Consumer**: Order Service

### Order Created Event
- **Exchange**: `order.events`
- **Routing Key**: `order.created`

## Development

### Chạy local (không dùng Docker)
```bash
# Terminal 1: Start RabbitMQ 4
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:4-management

# Terminal 2: User Service
cd Services/UserService
dotnet run

# Terminal 3: Order Service  
cd Services/OrderService
dotnet run

# Terminal 4: Web Client (optional - served via nginx in Docker)
# Serve WebClient directory with any web server
```

### Logs
```bash
# Xem logs tất cả services
docker-compose logs -f

# Xem logs từng service
docker-compose logs -f userservice
docker-compose logs -f orderservice
docker-compose logs -f rabbitmq
docker-compose logs -f webclient
docker-compose logs -f nginx-gateway
```

## 🌐 Web Interface

### Features
- **User Management**: Tạo, xem danh sách users
- **Order Management**: Tạo đơn hàng, theo dõi trạng thái
- **Payment Processing**: Xử lý thanh toán với visual feedback
- **Real-time Updates**: AJAX-based interactions
- **Responsive Design**: Hoạt động trên mobile và desktop

### Usage
1. Truy cập http://localhost:8080
2. Tạo users trong tab "Users"  
3. Tạo orders trong tab "Orders"
4. Xử lý thanh toán bằng nút "Pay Now"
5. Theo dõi trạng thái thanh toán real-time

## Monitoring & Management

- **Web Application**: http://localhost:8080
- **RabbitMQ Management UI**: http://localhost:15672 (guest/guest)  
- **Swagger UI User Service**: http://localhost:5001/swagger
- **Swagger UI Order Service**: http://localhost:5002/swagger

### RabbitMQ 4 Management Features
- **Queue monitoring**: Theo dõi message queues
- **Exchange management**: Quản lý exchanges và bindings
- **Connection tracking**: Theo dõi kết nối từ services
- **Performance metrics**: CPU, memory, throughput statistics

## Dọn dẹp

```bash
# Dừng tất cả services
docker-compose down

# Xóa volumes
docker-compose down -v

# Xóa images
docker-compose down --rmi all
```