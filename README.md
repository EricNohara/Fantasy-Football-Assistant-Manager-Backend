# Fantasy Football Assistant Manager Backend

### Contributors: Eric, Noah, Temi, Zac

## Table of Contents

- [Project Overview](#project-overview)
- [Project Purpose](#project-purpose)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Setup & Installation](#setup--installation)
- [API Endpoints](#api-endpoints)
- [Testing](#testing)

## Project Overview

This is the research project demo version of the backend for our fantasy football assistant manager app. The repo for the frontend can be found here:
https://github.com/EricNohara/Fantasy-Football-Assistant-Manager-Frontend

## Project Purpose

The purpose of this research project was to explore the tech stack and API options available to us and test out implementing our selections. More specifically, we have researched databases, AI APIs, NFL statistic APIs, and payment APIs. We selected and tested Supabase, OpenAI API, NFLVerse API, and Stripe API. The version of our app on this test branch contains basic functionality tests for each of these four components.

## Features

- Swagger UI for testing APIs
- Card payment frontend UI for mock Stripe payments
- Supabase database table querying
- ChatGPT querying
- Retrieving and parsing data from NFLVerse
- Making mock payments to Stripe

## Tech Stack

- **Backend Framework:** ASP.NET Core Web API (.NET 8)
- **Database:** Supabase (PostgreSQL)
- **Authentication:** Supabase JWT Authentication
- **Language:** C#
- **Version Control:** Git

## Setup & Installation

The backed and frontend code can be downloaded from this repository and  
https://github.com/EricNohara/Fantasy-Football-Assistant-Manager-Frontend  
respectively. On downloading, navigate to each solution directory from the command line and enter "dotnet restore" to install all necessary packages. Obtain your own API keys and accounts for Supabase, NFLVerse, OpenAI, and Stripe. Create the file "appsettings.json" with body:

```
{
  //supabase keys
  "Supabase": {
    "Url": ,
    "ServiceRoleKey":
  },
  //stripe keys
  "Stripe": {
    "PublishableKey": ,
    "SecretKey": ,
    "WebhookSecret":
  },
  "OpenAI": {
    "ApiKey":
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },

  //Indicates hosts from which requests can be accepted. Currently set to accept all.
  "AllowedHosts": "*"
}
```

Fill in all empty fields with your API keys.
In order to test Stripe webhooks, you will have to download the Stripe CLI. More information can be found here:
https://docs.stripe.com/stripe-cli  
After downloading, open the command line and run "stripe login". Follow the instructions that appear to link this machine to your Stripe account.

## Api Endpoints

| **Category**      | **Method** | **Endpoint**                                  | **Description**                                                                                |
| ----------------- | ---------- | --------------------------------------------- | ---------------------------------------------------------------------------------------------- |
| **ChatGPT**       | `POST`     | `/api/ChatGPT/test`                           | Test route that queries the OpenAI API with a static prompt.                                   |
| **ChatGPT**       | `POST`     | `/api/ChatGPT/chat`                           | Sends a custom query to the OpenAI API.                                                        |
| **Players**       | `POST`     | `/api/Players/all`                            | Fetches all offensive players from NFL Verse, parses the data, and inserts them into Supabase. |
| **Players**       | `DELETE`   | `/api/Players/all`                            | Deletes all players from Supabase.                                                             |
| **Stripe**        | `POST`     | `/api/Stripe/create-payment-intent`           | Creates Stripe payment intent and sends to client.                                             |
| **StripeWebhook** | `POST`     | `/api/StripeWebhook/check-payment-completion` | Webhook checking Stripe for if a payment was completed.                                        |
| **SupaBase**      | `GET`      | `/api/SupaBase/{table}`                       | Gets the data from a specified table from Supabase.                                            |
| **Users**         | `POST`     | `/api/Users/signup`                           | Creates a new user in Supabase.                                                                |

# Testing

Most features of this test implementation can be tested using the **auto-generated Swagger UI**.

To start the backend:

```bash
dotnet run --launch-profile "https"
```

Once running, copy the **local URL** displayed in the console and open it in your browser — then append `/swagger` to the end.

```
https://localhost:5001/swagger
```

This will open the **Swagger testing interface**.

---

## Supabase Testing

1. In Swagger, select the **Supabase** endpoint.
2. Click **"Try it out"**.
3. In the _table_ field, enter the name of any table in the project database schema (e.g., `"Users"`).
4. Click **"Execute"**.

This will retrieve all entries from the given table.

> **Note:** The Supabase database is not yet fully populated.

---

## ChatGPT Testing

1. Select the **ChatGPT Test** endpoint.
2. Click **"Try it out"**.
3. Enter a quotation-enclosed string (e.g., `"Hello ChatGPT!"`).
4. Click **"Execute"**.

**Sample Query:**

```
{
  "message": "Return a json object with fields whoToStart and reasoning: {\"player_one\": {\"name\": \"Aaron Rodgers\", \"position\": \"QB\", \"team\": \"PIT\", \"completions\": 74, \"attempts\": 108, \"passing_yards\": 786, \"passing_tds\": 8, \"passing_interceptions\": 3, \"sacks_against\": 9, \"passing_air_yards\": 515, \"passing_first_downs\": 31, \"passing_epa\": 6.51129352118511, \"carries\": 9, \"rushing_yards\": 11, \"rushing_tds\": 0, \"fumbles\": 1, \"rushing_first_downs\": 1, \"rushing_epa\": -2.51797030409565, \"fantasy_points\": 60.54, \"fantasy_points_ppr\": 60.54}, \"player_two\": {\"name\": \"Daniel Jones\", \"position\": \"QB\", \"team\": \"IND\", \"completions\": 87, \"attempts\": 121, \"passing_yards\": 1078, \"passing_tds\": 4, \"passing_interceptions\": 2, \"sacks_against\": 4, \"passing_air_yards\": 1013, \"passing_first_downs\": 52, \"passing_epa\": 38.9130099857412, \"carries\": 18, \"rushing_yards\": 54, \"rushing_tds\": 3, \"fumbles\": 1, \"rushing_first_downs\": 8, \"rushing_epa\": 8.26075707325101, \"fantasy_points\": 78.52, \"fantasy_points_ppr\": 78.52}}"
}
```

The response will display ChatGPT’s reply to your input string.

---

## NFLVerse Testing

1. Select the **Players (POST)** endpoint.
2. Click **"Try it out"**, then **"Execute"**.
3. This retrieves all NFL player data from **NFLVerse** and stores it in the project **Supabase** database.
4. To confirm, use the Supabase testing steps above and enter the table name `"Players"`.

---

## Stripe Testing

To view **Stripe event logs**, stop the backend and set the environment to Development:

```bash
set ASPNETCORE_ENVIRONMENT=Development
```

This enables debug logs on the console.

> **Note:** Stripe testing requires both the backend and frontend to be running.

### 1. Start the Stripe listener

Open a terminal and enter:

```bash
stripe listen --forward-to https://localhost:<local port>/api/stripewebhook/check-payment-completion
```

This will register your machine as a temporary webhook endpoint.

### 2. Test the payment flow

From the frontend UI, enter the following **test card info**:

| **Number**          | **Expiration Date**   | **CVC** |
| ------------------- | --------------------- | ------- |
| 4111 1111 1111 1111 | any future month/year | 111     |

This is a **fake credit card** for testing only.

Press **"Pay $10"** to initiate a payment.  
You should see a `PaymentIntent succeeded` log in your backend console.  
Your Stripe dashboard should show a successful $10 test payment.
