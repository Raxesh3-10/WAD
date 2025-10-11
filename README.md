# 🏫 School Administrative System (SAS)

## 📘 Project Overview
The **School Administrative System (SAS)** is a web-based management system built using **ASP.NET Core MVC with Entity Framework**.  
It automates major school administrative functions such as managing students, notices, bills, and role-based dashboards for different users.  

The system provides separate functionalities for **Principal**, **Teacher**, **Staff**, and **Trustee** roles to maintain a clear and structured workflow within the school.

This project is developed as part of the **Web Application Development (WAD)** coursework at **Dharmsinh Desai University (DDU)**.

---

## 👨‍💻 Team Members
This project is created by three students of sem-V class-B2 year-2025 as part of a team effort:

| Roll No. | Name | Role |
|-------------|------|------|
| **CE-115** | **Ravda Hasan** | Developer |
| **CE-122** | **Parmar Raxesh** | Developer |
| **CE-124** | **Patel Aksh** | Developer |

---

## ⚙️ Key Features

### 🧑‍💼 Roles & Permissions
The system includes **four user roles**, each with specific permissions and dashboards:

| Role | Permissions |
|------|--------------|
| **Trustee** | Can **create, update, delete** notices; can **view principal**, **all staff**, and **all teacher salaries**; can **view all user details** (but trustee does not have personal details). |
| **Principal** | Can **create, update, delete** notices and students; can **view bills**; can **download timetable (PDF)**; can **download ID cards (Principal, Teacher, Staff, Student)**; can **perform “Year Finish”** to increment all students’ standard by one. |
| **Teacher** | Can **view notices**; can **create, update, and delete students**. |
| **Staff** | Can **view notices**; can **create, update, and delete bills**. |

---

## 🧩 Major Modules

### 📰 Notice Management
- Trustee and Principal can manage (create/update/delete) notices.
- Notices are viewable by all roles.

### 💵 Bill Management
- Staff can manage bills using CRUD operations.
- Bills are viewable by Principal for record purposes.

### 👨‍🎓 Student Management
- Teachers and Principals can manage student details.
- Includes **Student** and **PreviousStudent** models with repositories and partial views.

### 🕒 Timetable & ID Cards
- Principal can download timetable and ID cards (Student, Teacher, Staff, Principal) as **PDF files**.

### 🔄 Year Finish
- One-click feature for the Principal to promote all students to the next standard at the end of the academic year.

### 🌐 Cloudinary & Email Integration
- **Cloudinary** used for storing and retrieving uploaded images/files.
- **Mail** feature for sending automated emails (e.g., notifications, communication).

---

## 🧱 Tech Stack

| Layer | Technology Used |
|-------|------------------|
| **Frontend** | Razor Pages / HTML / CSS / Bootstrap |
| **Backend** | ASP.NET Core MVC |
| **Database** | Microsoft SQL Server with Entity Framework Core |
| **Language** | C# |
| **Version Control** | Git & GitHub |
| **IDE** | Visual Studio 2022 |

---

## 🚀 Setup Instructions

### Prerequisites
Make sure you have the following installed:
- [.NET Core SDK 3.1](https://dotnet.microsoft.com/en-us/download/dotnet/3.1)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)

---

### 🔧 Project Setup

1. **Create a new ASP.NET Core MVC project** using **.NET Core 3.1**.

2. **Add the following NuGet dependencies** in your `.csproj` file:
   ```xml

   <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="3.1.0" />
   <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
   <PackageReference Include="CloudinaryDotNet" Version="1.27.6" />
   <PackageReference Include="Microsoft.EntityFrameworkCore" Version="3.1.1" />
   <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="3.1.1" />
   <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="3.1.1">
     <PrivateAssets>all</PrivateAssets>
     <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
   </PackageReference>
   <PackageReference Include="NETCore.MailKit" Version="2.1.0" />
   <PackageReference Include="NodaTime" Version="3.2.2" />
   <PackageReference Include="PuppeteerSharp" Version="20.2.2" />
   <PackageReference Include="QuestPDF" Version="2025.7.0" />

    ```
3. **Copy the following folders** from the project:

   ```
   Config
   Models
   ViewModels
   Views
   Controllers
   Mapper
   Services
   Repositories
   ViewComponents
   ```

4. **In `wwwroot`**, copy the two **img** folders used in the original project.

5. **Add the following essential files**:

   * `AppDbContext.cs`
   * `Program.cs`
   * `Startup.cs`
   * `appsettings.json`

6. **Update your `appsettings.json`** with Cloudinary and Mail credentials:

   ```json
   "Cloudinary": {
     "CloudName": "",
     "ApiKey": "",
     "ApiSecret": ""
   },

   "MailSettings": {
     "FromEmail": "",
     "AppPassword": "",
     "DevEmail": ""
   }
   ```

7. **Run EF Core Migrations:**

   ```bash
   Add-Migration InitialCreate
   Update-Database
   ```

8. **Run the Project:**

   ```bash
   dotnet run
   ```

   or simply press **F5** in Visual Studio.

---

## 👥 Individual Contributions

### 🧑‍💻 CE-115 — **Ravda Hasan**

* Integrated **Cloudinary** for file uploads
* Implemented **Mail sending functionality**
* Designed **Navigation Bar**, **_Layout page**, **Footer**, and **Home Page**
* Developed **Trustee Dashboard**, **Signup Page**, and **Login Page**

### 🧑‍💻 CE-124 — **Patel Aksh**

* Implemented **Notice** and **Bills** modules (Models, Repository, Partial Views, ViewComponents)
* Developed **Staff** and **Teacher Dashboards**
* Created **Report Bug Page** and related **ViewComponents**

### 🧑‍💻 CE-122 — **Parmar Raxesh**

* Developed **User** and **UserDetails** models and repositories
* Created **Details Page** and **Principal Dashboard**
* Implemented **Student** and **PreviousStudent** models, repositories, and partial views
* Built **ViewComponents**, **Timetable**, and **ID Card (PDF)** features

---

> *Developed by Team SAS – CE-115, CE-122, and CE-124*
> *A role-based school management system built with ASP.NET Core MVC and EF Core.*