# 🚀 **RABBITMQ IMPLEMENTATION GUIDE**

## 📋 **ARQUITETURA COMPLETA**

✅ **API Gateway** - Porta 5000 (Autenticação JWT)  
✅ **Sales.API** - Porta 5262 (Gestão de vendas + Publisher RabbitMQ)  
✅ **Stock.API** - Porta 5263 (Gestão de estoque + Consumer RabbitMQ)  
✅ **RabbitMQ** - Porta 5672 (AMQP) + 15672 (Management UI)  
✅ **MySQL** - Porta 3306 (Bancos: sales_db, stock_db)  

---

## 🐰 **RABBITMQ REAL IMPLEMENTATION**

### **1. Infraestrutura Docker**
```bash
# Iniciar apenas RabbitMQ + MySQL
cd EcommerceMicroservices
docker-compose -f docker-compose.dev.yml up -d

# Verificar status
docker-compose -f docker-compose.dev.yml ps
```

### **2. Acessos**
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **MySQL**: localhost:3306 (developer/Luke@2020)

### **3. Filas Criadas Automaticamente**
- `stock-update-queue` - Atualizações de estoque
- `order-created-queue` - Notificações de pedidos

---

## 🔄 **FLUXO COMPLETO DE COMUNICAÇÃO**

### **Sales.API → RabbitMQ → Stock.API**

1. **Cliente cria pedido** via Sales.API
2. **Sales.API valida estoque** via HTTP (Stock.API)
3. **Sales.API salva pedido** no banco sales_db
4. **Sales.API publica mensagem** no RabbitMQ (stock-update-queue)
5. **Stock.API consome mensagem** e atualiza estoque
6. **Stock.API salva alteração** no banco stock_db

---

## 🛠️ **EXECUTAR SISTEMA COMPLETO**

### **Passo 1: Infraestrutura**
```bash
docker-compose -f docker-compose.dev.yml up -d
```

### **Passo 2: Microservices**
```bash
# Terminal 1 - API Gateway
cd API.Gateway
dotnet run

# Terminal 2 - Stock.API
cd Stock.API
dotnet run

# Terminal 3 - Sales.API
cd Sales.API
dotnet run
```

---

## 🧪 **TESTAR IMPLEMENTAÇÃO**

### **1. Autenticar**
```bash
POST http://localhost:5000/api/auth/login
{
  "email": "admin@test.com",
  "password": "password123"
}
```

### **2. Criar Pedido (Trigger RabbitMQ)**
```bash
POST http://localhost:5000/api/orders
Authorization: Bearer {token}
{
  "customerName": "João Silva",
  "customerEmail": "joao@email.com",
  "items": [
    {
      "productId": 1,
      "quantity": 2,
      "unitPrice": 29.99
    }
  ]
}
```

### **3. Verificar Logs**
- **Sales.API**: Mensagem publicada no RabbitMQ
- **Stock.API**: Mensagem consumida e estoque atualizado
- **RabbitMQ UI**: Filas ativas e mensagens processadas

---

## 🔍 **STATUS DA IMPLEMENTAÇÃO**

### ✅ **COMPLETO**
- Microservices funcionais
- JWT Authentication
- Entity Framework + MySQL
- HTTP comunicação entre APIs
- **RabbitMQ REAL com fallback para mock**

### ⚠️ **COMPORTAMENTO**
- Se RabbitMQ estiver rodando → **Comunicação real via filas**
- Se RabbitMQ não estiver disponível → **Fallback para modo mock (apenas logs)**

---

## 🎯 **PRÓXIMOS PASSOS**

1. **Executar infraestrutura** (docker-compose.dev.yml)
2. **Executar microservices** (dotnet run em cada um)
3. **Testar fluxo completo** (autenticar → criar pedido → verificar logs)
4. **Monitorar RabbitMQ** (http://localhost:15672)

---

## 🔧 **TROUBLESHOOTING**

### **RabbitMQ não conecta**
- Verificar se Docker está rodando
- Verificar porta 5672 livre
- Checar logs: `docker logs ecommerce-rabbitmq`

### **MySQL não conecta**
- Verificar porta 3306 livre
- Checar credenciais nos appsettings.json
- Verificar logs: `docker logs ecommerce-mysql`
