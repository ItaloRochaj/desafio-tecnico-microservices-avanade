-- Criar as bases de dados para os microservices
CREATE DATABASE IF NOT EXISTS sales_db;
CREATE DATABASE IF NOT EXISTS stock_db;

-- Garantir que o usuário developer tenha permissões
GRANT ALL PRIVILEGES ON sales_db.* TO 'developer'@'%';
GRANT ALL PRIVILEGES ON stock_db.* TO 'developer'@'%';
FLUSH PRIVILEGES;
