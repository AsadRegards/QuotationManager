# 🧾 Quotation Manager

A web-based quotation management system built with ASP.NET Core MVC that allows users to create, preview, and manage product quotations, including automatic PDF generation with product images, signatures, and company stamps.

---

## 🚀 Features

- **User Registration & Authentication**
  - Secure signup and login.
  - Users upload a signature and company stamp image during signup (optional).
  - A placeholder white image can be used if no stamp/signature is needed.

- **Product Management**
  - Add products with name, unit price, description, and image.
  - Live preview of how each product will appear in the final quotation.

- **Quotation Creation**
  - Create quotations using added products.
  - Auto-generated reference number and editable GST percentage.
  - Add one or more products with quantity to a single quotation.
  - Real-time live preview of quotation structure and layout.

- **Quotation Formatting Rules**
  - Each product with an image gets its own page.
  - Products without images are grouped and shown on the last page.
  - Signature and stamp (if provided) appear on every product page.

- **User-Specific Views**
  - Users can only view quotations and products they’ve created.
  - Quotations are read-only once created (can be edited but not deleted).
  - Products can be edited or deleted anytime.

---

## 🛠️ Tech Stack

- **Backend:** ASP.NET Core MVC (.NET 7)
- **Frontend:** Razor Pages (Bootstrap for styling)
- **Database:** Entity Framework Core with SQL Server
- **PDF Generation:** [QuestPDF](https://www.questpdf.com/)
- **Authentication:** ASP.NET Identity

---

## 🤖 AI Involvement

This project was heavily assisted by **ChatGPT** (OpenAI) during development:
- ~80% of the code was generated with the help of AI prompts.
- ChatGPT assisted with structuring controllers, formatting PDF layouts, optimising Razor views, and resolving development blockers.

This project showcases how AI tools can enhance developer productivity while maintaining full control and customisation over application logic.

---

## 📦 Installation & Setup (Optional)


1. Clone the repo  
   ```bash
   git clone https://github.com/AsadRegards/QuotationManager.git
   cd QuotationManager
   ```
2. Make sure you have SQL Server installed locally or remotely (in the cloud) and have an accessible connection string.
3. Edit appsettings.json to replace the connectionString with your connection string.
4. Open Package Manager Console and run the following command to Create Migrations and update the database.
  ```base
  Add-Migration Initial
  Update-Database
  ```
5. Before starting the project, make sure to replace the default-letterhead.jpg with your letterhead image (or you can leave it as it is if you are just testing)
5. Now start the project and create the first user, first product and first quotation.
   


