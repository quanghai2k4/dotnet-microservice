# .NET Microservice with RabbitMQ, Docker & NGINX

Dự án microservice .NET sử dụng RabbitMQ, Docker và NGINX API Gateway.

## Cấu trúc dự án

```
Microservice/
├── Services/
│   ├── UserService/          # Microservice quản lý người dùng
│   └── OrderService/         # Microservice quản lý đơn hàng
├── Common/
│   └── RabbitMQ.Client/      # Thư viện RabbitMQ chung
├── Gateway/
│   └── nginx/                # NGINX API Gateway
└── docker-compose.yml        # Docker orchestration
```

## Công nghệ sử dụng

- **.NET 8**: Framework chính
- **RabbitMQ Client 7.1.2**: Message broker native client
- **Docker & Docker Compose**: Container orchestration
- **NGINX**: API Gateway và Load Balancer
- **Entity Framework Core**: ORM (In-Memory Database)
- **Swagger/OpenAPI**: API Documentation

## Khởi chạy dự án

### Yêu cầu
- Docker Desktop
- .NET 8 SDK (để development)

### Chạy toàn bộ hệ thống
```bash
docker-compose up --build
```

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
- **API Gateway**: http://localhost:8080
- **User Service**: http://localhost:8080/api/users
- **Order Service**: http://localhost:8080/api/orders
- **Health Check**: http://localhost:8080/health

### Direct Service Access
- **User Service**: http://localhost:5001
- **Order Service**: http://localhost:5002
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
# Terminal 1: Start RabbitMQ
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.12-management

# Terminal 2: User Service
cd Services/UserService
dotnet run

# Terminal 3: Order Service  
cd Services/OrderService
dotnet run
```

### Logs
```bash
# Xem logs tất cả services
docker-compose logs -f

# Xem logs từng service
docker-compose logs -f userservice
docker-compose logs -f orderservice
docker-compose logs -f rabbitmq
```

## Monitoring

- **RabbitMQ Management UI**: http://localhost:15672
- **Swagger UI User Service**: http://localhost:5001/swagger
- **Swagger UI Order Service**: http://localhost:5002/swagger

## Dọn dẹp

```bash
# Dừng tất cả services
docker-compose down

# Xóa volumes
docker-compose down -v

# Xóa images
docker-compose down --rmi all
```