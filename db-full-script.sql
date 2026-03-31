-- USERS (у тебя уже есть, но на всякий случай)
CREATE TABLE users (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    full_name VARCHAR(100),
    email VARCHAR(100),
    password_hash VARCHAR(255),
    role VARCHAR(50),
    created_at DATETIME,
    phone VARCHAR(20),
    avatar_path VARCHAR(255)
);

--------------------------------------------------

-- REQUESTS
CREATE TABLE requests (
    request_id INT IDENTITY(1,1) PRIMARY KEY,
    customer_name NVARCHAR(MAX) NOT NULL,
    customer_email NVARCHAR(MAX) NOT NULL,
    customer_phone NVARCHAR(MAX),
    status NVARCHAR(MAX) NOT NULL,
    
    -- ⚠️ лучше INT, а не TEXT
    assigned_to INT NULL,
    
    total DECIMAL(18,2) NOT NULL,
    created_at DATETIME NOT NULL,

    CONSTRAINT FK_requests_users 
        FOREIGN KEY (assigned_to) REFERENCES users(user_id)
);

--------------------------------------------------

-- REQUEST ITEMS
CREATE TABLE request_items (
    request_item_id INT IDENTITY(1,1) PRIMARY KEY,
    request_id INT NOT NULL,
    title NVARCHAR(MAX),
    price DECIMAL(18,2) NOT NULL,

    CONSTRAINT FK_request_items_requests 
        FOREIGN KEY (request_id) 
        REFERENCES requests(request_id)
        ON DELETE CASCADE
);

--------------------------------------------------

-- REQUEST STATUSES (если используешь)
CREATE TABLE request_statuses (
    status_entity_id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    color VARCHAR(20) NOT NULL,
    icon VARCHAR(50) NOT NULL
);

--------------------------------------------------

-- REVIEWS
CREATE TABLE reviews (
    id INT IDENTITY(1,1) PRIMARY KEY,
    author_id NVARCHAR(MAX) NOT NULL,
    author_name NVARCHAR(MAX) NOT NULL,
    text NVARCHAR(1000) NOT NULL,
    rating INT NOT NULL,
    created_at DATETIME NOT NULL
);