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

## 📷 Screenshots

_Add screenshots here of:_
- Product creation form with live preview
- Quotation live preview
- Sample exported PDF

---

## 📦 Installation & Setup (Optional)

> **Note:** This is a basic guide. Full setup instructions can be added later.

1. Clone the repo  
   ```bash
   git clone https://github.com/AsadRegards/QuotationManager.git
   cd QuotationManager
