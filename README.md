# Overview

Backend of my **thesis work**: payment facilitator and pass handling application for gyms.

Provides server side features for the application using [ASP.NET Core](http://asp.net/). Works in demo mode only.

The solution was initially scaffolded using [Jason Taylor's Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture) version 9.0.12, and later it was upgraded to .NET 10, because of this and due to the scale of the application, the backend structure follows Clean Architecture and its principles such as separation of concerns, maintainability, testability etc.

**First and second semester presentation can be found in [docs folder](./docs).** Unfortunately the final thesis presentation is *lost*.

---

## Features

### [`Gym`](./src/Domain/Entities/Gym.cs)
- Update operations for `GymAdministrator` and `AppAdministrator`
- [`GymStatus`](./src/Domain/Enums/GymStatus.cs) change will result in notification sent to all gym employee via the [`IEmailService`](./src/Application/Common/Interfaces/IEmailService.cs) and the [`IClientNotificationSender`](./src/Application/Common/Interfaces/IClientNotificationService.cs) - which sends a real time notification to the frontend via `TypedResults.ServerSentEvents`

### [`Request`](./src/Domain/Entities/Request.cs)
- Built-in ticket system of the application
- [`Request`](./src/Domain/Entities/Request.cs) objects can be created by `User`, `GymStaff` and `GymAdministrator` users
- Created ones are managed by `AppAdministrator` users:
  - Reject them
  - Accept them. E.g: accepting a [`Request`](./src/Domain/Entities/Request.cs) with `GymCreation` [`RequestType`](./src/Domain/Enums/RequestType.cs) will result in creating the specified [`Gym`](./src/Domain/Entities/Gym.cs) and promoting the [`Request`](./src/Domain/Entities/Request.cs) creator to the `GymAdministrator` role

### `PaymentIntent`
- Payments are handled through payment intents:
  1. Frontend sends a request to the backend to create a payment intent for a `User` & a [`GymPassProduct`](./src/Domain/Entities/GymPassProduct.cs) => metadata will be attached to the Stripe `PaymentIntent`
  2. Backend responds with a [`PaymentIntentDto`](./src/Application/PaymentIntents/DTOs/PaymentIntentDto.cs) containing a client secret
  3. The acquired client secret will be used to display the required purchase components for the UI
  4. The result of the payment will be sent be via a `StripeEvent` to the backend which can serve accordingly
  5. In case of success the `User` will receive a [`GymMembershipPass`](./src/Domain/Entities/GymMembershipPass.cs) according to the purchased [`GymPassProduct`](./src/Domain/Entities/GymPassProduct.cs) and the frontend will receive an event to notify the user an invalidate frontend caches // In case of error the `User` will receive notifications and an error will be logged

### [`GymPassProduct`](./src/Domain/Entities/GymPassProduct.cs)
- Create, update and delete operations authorized to `GymAdministrator`
- Querying for all users
- Event handlers for successful and failed purchase webhooks coming from [Stripe](https://stripe.com/)
- A purchased [`GymPassProduct`](./src/Domain/Entities/GymPassProduct.cs) will be used to create a [`GymMembershipPass`](./src/Domain/Entities/GymMembershipPass.cs) and other metadata contained by the [Stripe](https://stripe.com/) event will be used to assign it to the correct `User`

### [`GymMembership`](./src/Domain/Entities/GymMembership.cs)
- Holds purchased passes of an [`ApplicationUser`](./src/Infrastructure/Identity/ApplicationUser.cs)
- Also describes the [status](./src/Domain/Enums/GymMemberStatus.cs) of the user in the specified [`Gym`](./src/Domain/Entities/Gym.cs)
- The status of a membership can be changed by a user in a gym employee role (see: [`Roles`](./src/Domain/Constants/Roles.cs))
- This status can be `Banned` or `Active`
  - `Banned` status prevents the user from using their pass in the [`Gym`](./src/Domain/Entities/Gym.cs)
- In case of a status change the affected user is notified via email and client notification

### [`GymMembershipPass`](./src/Domain/Entities/GymMembershipPass.cs)
- Created by the backend according to the purchased [`GymPassProduct`](./src/Domain/Entities/GymPassProduct.cs)
- It can be verified whether it is valid or not by a gym employee - or also used by them to start a gym session for the `User`

### [`GymEmployment`](./src/Domain/Entities/GymEmployment.cs)
- Holds values of a `GymStaff` or `GymAdministrator` user's employment in a [`Gym`](./src/Domain/Entities/Gym.cs)
- These objects are handled by the backend

### [`GymContactInfo`](./src/Domain/Entities/GymContactInfo.cs)
- CRUD operations for the [`GymContactInfo`](./src/Domain/Entities/GymContactInfo.cs) entity
- Describes [`Gym`](./src/Domain/Entities/Gym.cs) contact information such as email or [`PhoneNumber`](./src/Domain/ValueObjects/PhoneNumber.cs)

### [`ApplicationUser`](./src/Infrastructure/Identity/ApplicationUser.cs) & [`UserProfile`](./src/Domain/Entities/UserProfile.cs)
- Identity features are hand built instead of using the Identity package features for education purposes:
  - Registration
  - Role handling
  - Password reset
  - etc.

**For further technical details read the codebase or contact University of Pannonia for the hungarian thesis.**

---

## Structure

TODO

---

## Run the app

- Application requires .NET version: 10.0.100
- Running the app also requires API keys
- API keys are loaded from secrets.local.json in [Web project root](src/Web) for **simplicity**
- Database type can be set to `InMemory` or `SQL` in appsettings.Development.json
- If SQL DbType is set the database will be created automatically using the given connection string in appsettings.Development.json

### Prerequisites
- Stripe Secret Key & webhook secret (from [StripeDashboard](https://dashboard.stripe.com/apikeys))
- Stripe Webhook endpoint url: https://localhost:5001/api/Webhooks/Stripe

**Add the secrets.local.json file with the following structure using your own API keys:**

<pre>
{
    "Stripe": {
        "Key": "insert_key",
        "WebhookSecret": "insert_key"
    },
    "Jwt": {
        "Key": "JG3Auz1bttDL8k9AX8PVC4LByiAveG0i+IoeFbSdCc8="
    }
}
</pre>

### Run app with Payments (test mode only)
- Download [Stripe CLI](https://github.com/stripe/stripe-cli/releases/tag/v1.35.1)
- in terminal run: stripe login --interactive
- provide tes API key: sk_test_...
- run: stripe listen --forward-to https://localhost:5001/api/Webhooks/Stripe --skip-verify
- after this payment related webhooks are forwarded to the correct endpoint to fulfill purchases

### Run automatic tests:
- Domain.UnitTests & Application.UnitTests do not require anything specific
- Application.FunctionalTests require Docker engine running (tested via SQL & Stripe containers) & appsettings.Testing.json: "DbType": "SQL"
- secrets.local.json is not required for testing