# 🏢 Freelance Job Board  
The **Freelance Job Board** is a full-stack web application that connects **Clients** and **Freelancers**, enabling seamless collaboration from **job posting** to **contract completion**.  

 Built using **.NET Core Web API, EF Core, SignalR, SQL Server, Clean Architecture, and CQRS with Mediator Pattern**.  
Provides a **scalable, maintainable, and production-ready** platform with real-time notifications.  

---

## 📚 Table of Contents  
- [Features](#-features)  
- [Architecture](#-architecture)  
- [Tech Stack](#-tech-stack)  
- [Getting Started](#-getting-started)  
- [Screenshots](#-screenshots)  
- [License](#-license)  


## 🔑 Features  

### 👤 Authentication & Profiles  
- Role-based access: **Client**, **Freelancer**, **Admin**  
- Register/Login with JWT + Refresh Tokens  
- Profile management (bio, skills, portfolio, company details)  
- File uploads for profile images & portfolios  

### 💼 Jobs & Applications  
- Clients: Create, edit, delete job postings  
- Job details: title, budget, deadline, categories, skills, attachments  
- Freelancers: Apply with proposal (cover letter, bid, timeline)  
- Application workflow: Submitted → Under Review → Accepted/Rejected  

### ⭐ Reviews & Ratings  
- Clients and Freelancers can **rate each other** after job completion  
- Ratings displayed on profiles  

### 📊 Admin Dashboard  
- View platform statistics (jobs, users, top clients)  
- Moderate job postings (approve/reject)  
- Manage applications & ratings  

### 🔔 Real-time Notifications (SignalR)  
- **Clients**: notified when proposals are received, jobs approved, contracts updated  
- **Freelancers**: notified when proposals are accepted/rejected, contracts updated  
- **Admins**: notifications for moderation actions  

## 🛠 Tech Stack  

- **Backend**: .NET Core Web API, EF Core, SignalR  
- **Frontend**: ASP.NET MVC  
- **Database**: SQL Server  
- **Auth**: JWT + Refresh Tokens  
- **Testing**: XUnit + Moq + Fluent Assertions  


## 🏗 Architecture  

- **Clean Architecture** (Onion-based) for maintainability  
- **CQRS + Mediator Pattern** for commands & queries separation  
- **Repository + Unit of Work** for persistence  
- **Middlewares**:  
  - Exception Handling Middleware (centralized error handling)  
  - Logging with **Serilog**  
  - Rate Limiting  


## 📸 Screenshots  
Here are some screenshots of the application: 

![Home Page](screen%20shots/home.png)
![Job Management - Admin](screen%20shots/job%20management%20(Admin%20Dashboard).png)
![Proposal Submitted](screen%20shots/proposal-submitted-to-client.png)
![Contract](screen%20shots/contract-between-client-freelancer.png)
![Client Notifications](screen%20shots/client-notifications.png)
![Freelancer Notifications](screen%20shots/freelancer-notification.png)

## 🚀 Getting Started  

### Prerequisites  
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)  
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)  

### Clone & Setup  
```bash
# Clone repo
git clone https://github.com/your-username/FreelanceJobBoard.git

# Navigate to project
cd FreelanceJobBoard

# Apply migrations
dotnet ef database update

# Run API
dotnet run --project FreelanceJobBoard.API

# Run MVC frontend
dotnet run --project FreelanceJobBoard.MVC
