

# ☕ E-Commerce Coffee Shop Application

An online coffee shop built with **ASP.NET Core MVC** on **.NET 8**, featuring a fully functional product catalog, session-based shopping cart, secure authentication, and a responsive, modern design. This application demonstrates clean architectural organization and real-world e-commerce functionality.


---

## Features

* Full CRUD functionality for managing products
* Session-based Shopping Cart with dynamic cart count tracking
* User Authentication using **ASP.NET Identity**
* Role-Based Access Control (Admin/User)
* Secure Checkout Flow
* Responsive UI built with **Bootstrap 5**
* Clean Architecture for maintainability and scalability

---

## Technologies Used

* .NET 8
* ASP.NET Core Razor Pages
* Entity Framework Core (Code-First)
* SQL Server
* ASP.NET Identity
* Bootstrap 5
* HTML / CSS / JavaScript
* Session State Management

---

## Getting Started

### Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
* [SQL Server](https://www.microsoft.com/en-us/sql-server/)
* Visual Studio 2022+ or VS Code

### Setup Instructions

1. Clone the repository

   ```bash
   git clone https://github.com/your-username/coffee-shop-app.git
   cd coffee-shop-app
   ```

2. Update database connection string in `appsettings.json`

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=CoffeeShopDb;Trusted_Connection=True;"
   }
   ```

3. Apply migrations and seed database

   ```bash
   dotnet ef database update
   ```

4. Run the application

   ```bash
   dotnet run
   ```
