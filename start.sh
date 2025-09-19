#!/bin/bash

echo "Building and starting .NET Microservices..."

# Build và khởi động tất cả services
docker-compose up --build -d

echo "Services started successfully!"
echo "API Gateway: http://localhost:8080"
echo "RabbitMQ Management: http://localhost:15672 (guest/guest)"
echo "User Service (direct): http://localhost:5001"
echo "Order Service (direct): http://localhost:5002"

echo ""
echo "Waiting for services to be ready..."
sleep 10

echo "Testing API endpoints..."
echo ""

echo "1. Creating a user..."
curl -X POST http://localhost:8080/api/users \
  -H "Content-Type: application/json" \
  -d '{"name": "John Doe", "email": "john@example.com"}' \
  -w "\nStatus: %{http_code}\n"

echo ""
echo "2. Getting all users..."
curl http://localhost:8080/api/users -w "\nStatus: %{http_code}\n"

echo ""
echo "3. Creating an order..."
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{"userId": 1, "productName": "Laptop", "quantity": 1, "price": 1000.00}' \
  -w "\nStatus: %{http_code}\n"

echo ""
echo "4. Getting all orders..."
curl http://localhost:8080/api/orders -w "\nStatus: %{http_code}\n"

echo ""
echo "Setup completed! Check the logs with: docker-compose logs -f"