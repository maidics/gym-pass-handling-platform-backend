# FitnessPayFac backend

The project was generated using the [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture) version 9.0.12, and later it was upgraded to .NET 10.

# Run the app

- Running the app requires API keys
- API keys are loaded from secrets.local.json in Web project root (src/Web/secrets.local.json).
- Database type can be set to InMemory or SQL in appsettings.Development.json

## Prerequisites
- Stripe Secret Key & webhook secret (from [StripeDashboard](https://dashboard.stripe.com/apikeys))
- Stripe Webhook endpoint url: https://localhost:5001/api/Webhooks/Stripe

**Add the secrets.local.json file with the following structure using your own API keys to access Stripe & Jwt services:**

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

## Run app with Payments (test mode only)
- Download [Stripe CLI](https://github.com/stripe/stripe-cli/releases/tag/v1.35.1)
- in terminal run: stripe login --interactive
- provide tes API key: sk_test_...
- run: stripe listen --forward-to https://localhost:5001/api/Webhooks/Stripe --skip-verify
- after this payment related webhooks are forwarded to the correct endpoint to fulfill purchases

# Run automatic tests:
- Domain.UnitTests & Application.UnitTests do not require anything specific
- Application.FunctionalTests require Docker engine running (tested via SQL & Stripe containers) & appsettings.Testing.json: "DbType": "SQL"
- secrets.local.json is not required for testing