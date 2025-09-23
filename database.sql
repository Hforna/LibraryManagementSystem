CREATE TYPE metodo_pagamento AS ENUM ('Dinheiro', 'Cartão de Crédito', 'Cartão de Débito', 'Pagamento Mobile');
CREATE TYPE cargo_funcionario AS ENUM ('Gerente', 'Vendedor', 'Caixa', 'Estoquista');


CREATE TABLE Autores (
    autor_id SERIAL PRIMARY KEY,
    primeiro_nome VARCHAR(50) NOT NULL,
    ultimo_nome VARCHAR(50) NOT NULL,
    biografia TEXT,
    nacionalidade VARCHAR(50)
);

CREATE TABLE Editoras (
    editora_id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    endereco TEXT,
    email_contato VARCHAR(100),
    telefone VARCHAR(15)
);

CREATE TABLE Categorias (
    categoria_id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    descricao TEXT
);

CREATE TABLE Livros (
    livro_id SERIAL PRIMARY KEY,
    titulo VARCHAR(200) NOT NULL,
    autor_id INT NOT NULL,
    editora_id INT NOT NULL,
    categoria_id INT NOT NULL,
    isbn VARCHAR(20) UNIQUE NOT NULL,
    ano_publicacao INT,
    preco NUMERIC(10, 2) NOT NULL,
    preco_custo NUMERIC(10, 2) NOT NULL,
    quantidade_estoque INT DEFAULT 0,
    limite_minimo_estoque INT DEFAULT 5,
    descricao TEXT,
    CONSTRAINT fk_autor FOREIGN KEY (autor_id) REFERENCES Autores(autor_id),
    CONSTRAINT fk_editora FOREIGN KEY (editora_id) REFERENCES Editoras(editora_id),
    CONSTRAINT fk_categoria FOREIGN KEY (categoria_id) REFERENCES Categorias(categoria_id)
);

CREATE TABLE Clientes (
    cliente_id SERIAL PRIMARY KEY,
    primeiro_nome VARCHAR(50) NOT NULL,
    ultimo_nome VARCHAR(50) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    telefone VARCHAR(15),
    endereco TEXT,
    data_registro DATE NOT NULL DEFAULT CURRENT_DATE,
    pontos_fidelidade INT DEFAULT 0
);

CREATE TABLE Funcionarios (
    funcionario_id SERIAL PRIMARY KEY,
    primeiro_nome VARCHAR(50) NOT NULL,
    ultimo_nome VARCHAR(50) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    telefone VARCHAR(15),
    endereco TEXT,
    cargo cargo_funcionario NOT NULL,
    data_contratacao DATE NOT NULL,
    salario NUMERIC(10, 2)
);

CREATE TABLE Vendas (
    venda_id SERIAL PRIMARY KEY,
    cliente_id INT,
    funcionario_id INT NOT NULL,
    data_venda TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    valor_total NUMERIC(10, 2) NOT NULL,
    metodo_pagamento metodo_pagamento NOT NULL,
    CONSTRAINT fk_cliente FOREIGN KEY (cliente_id) REFERENCES Clientes(cliente_id),
    CONSTRAINT fk_funcionario FOREIGN KEY (funcionario_id) REFERENCES Funcionarios(funcionario_id)
);

CREATE TABLE Detalhes_Venda (
    detalhe_venda_id SERIAL PRIMARY KEY,
    venda_id INT NOT NULL,
    livro_id INT NOT NULL,
    quantidade INT NOT NULL,
    preco_unitario NUMERIC(10, 2) NOT NULL,
    desconto NUMERIC(10, 2) DEFAULT 0.00,
    CONSTRAINT fk_venda FOREIGN KEY (venda_id) REFERENCES Vendas(venda_id) ON DELETE CASCADE,
    CONSTRAINT fk_livro_venda FOREIGN KEY (livro_id) REFERENCES Livros(livro_id)
);

CREATE TABLE Fornecedores (
    fornecedor_id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    pessoa_contato VARCHAR(100),
    email VARCHAR(100),
    telefone VARCHAR(15),
    endereco TEXT
);

CREATE TABLE Compras (
    compra_id SERIAL PRIMARY KEY,
    fornecedor_id INT NOT NULL,
    funcionario_id INT NOT NULL,
    data_compra DATE NOT NULL DEFAULT CURRENT_DATE,
    custo_total NUMERIC(10, 2) NOT NULL,
    CONSTRAINT fk_fornecedor FOREIGN KEY (fornecedor_id) REFERENCES Fornecedores(fornecedor_id),
    CONSTRAINT fk_funcionario_compra FOREIGN KEY (funcionario_id) REFERENCES Funcionarios(funcionario_id)
);

CREATE TABLE Detalhes_Compra (
    detalhe_compra_id SERIAL PRIMARY KEY,
    compra_id INT NOT NULL,
    livro_id INT NOT NULL,
    quantidade INT NOT NULL,
    custo_unitario NUMERIC(10, 2) NOT NULL,
    CONSTRAINT fk_compra FOREIGN KEY (compra_id) REFERENCES Compras(compra_id) ON DELETE CASCADE,
    CONSTRAINT fk_livro_compra FOREIGN KEY (livro_id) REFERENCES Livros(livro_id)
);