using System.Collections.Frozen;
using FitPass.Application.Common.Models;
using FitPass.Domain.ValueObjects;

namespace FitPass.Infrastructure.Stripe;

public static class MoneyExtensions
{
    extension(Money money)
    {
        public long ToStripeAmount()
        {
            ArgumentNullException.ThrowIfNull(money);

            // Logic: 
            // JPY (Zero decimal): 500 JPY -> 500
            // USD (2 decimal): $10.00 -> 1000
            return Money.IsZeroDecimal(money.Currency) ? 
                (long)Math.Round(money.Amount, MidpointRounding.AwayFromZero) :
                (long)Math.Round(money.Amount * 100, MidpointRounding.AwayFromZero);
        }

        public Result ValidateForStripe()
        {
            var currency = money.Currency.ToLowerInvariant();
            var stripeAmount = ToStripeAmount(money);

            if (!MinimumAmounts.TryGetValue(currency, out var minimum))
            {
                return Result.BusinessRuleViolation($"Currency '{currency}' is not currently supported for payment processing.");
            }

            if (stripeAmount < minimum)
            {
                var minimumMoney = FromStripeAmount(minimum, currency);

                return Result.BusinessRuleViolation(
                    $"Amount is below Stripe's minimum charge amount.",
                    [$"Required minimum amount for '{currency}' is {minimumMoney}."]);
            }

            return Result.Success();
        }
    }

    public static Money FromStripeAmount(long stripeAmount, string currency)
    {
        decimal amount;

        if (!Money.IsZeroDecimal(currency))
        {
            amount = stripeAmount / 100m;
        } else
        {
            amount = stripeAmount;
        }

        return new Money(amount, currency);
    }

    private static readonly FrozenDictionary<string, long> MinimumAmounts = new Dictionary<string, long>()
    {
        // --- North America ---
        { "usd", 50 },    // $0.50
        { "cad", 50 },    // $0.50
        { "mxn", 1000 },  // $10.00

        // --- Europe ---
        { "eur", 50 },    // €0.50
        { "gbp", 30 },    // £0.30
        { "chf", 50 },    // 0.50 Fr
        { "sek", 300 },   // 3.00 kr
        { "nok", 300 },   // 3.00 kr
        { "dkk", 250 },   // 2.50 kr
        { "pln", 200 },   // 2.00 zł
        { "czk", 1500 },  // ~15.00 Kč
        { "huf", 25000 }, // ~250.00 Ft
        { "ron", 300 },   // ~3.00 lei
        { "bgn", 100 },   // ~1.00 лв
        { "all", 6000 },  // ~60.00 L
        { "amd", 25000 }, // ~250.00 ֏
        { "bam", 100 },   // ~1.00 KM
        { "gel", 150 },   // ~1.50 ₾
        { "gip", 40 },    // £0.40
        { "mdl", 1000 },  // ~10.00 L
        { "mkd", 3000 },  // ~30.00 den
        { "rsd", 6000 },  // ~60.00 din
        { "uah", 2000 },  // ~20.00 ₴

        // --- Asia / Pacific ---
        { "aud", 50 },    // $0.50
        { "nzd", 50 },    // $0.50
        { "jpy", 50 },    // ¥50 (Zero Decimal)
        { "cny", 400 },   // ~¥4.00
        { "hkd", 400 },   // $4.00
        { "sgd", 50 },    // $0.50
        { "inr", 50 },    // ₹0.50
        { "idr", 1000000 }, // ~Rp 10,000
        { "krw", 700 },   // ~₩700 (Zero Decimal)
        { "myr", 200 },   // RM2.00
        { "php", 3000 },  // ~₱30.00
        { "thb", 2000 },  // ฿20.00
        { "vnd", 15000 }, // ~₫15,000 (Zero Decimal)
        { "pkr", 15000 }, // ~Rs 150.00
        { "bdt", 6000 },  // ~৳60.00
        { "lkr", 18000 }, // ~Rs 180.00
        { "mvr", 1000 },  // ~Rf 10.00
        { "npr", 7000 },  // ~Rs 70.00
        { "aed", 200 },   // 2.00 dr
        { "ils", 200 },   // ₪2.00
        { "sar", 200 },   // 2.00 SR
        { "qar", 200 },   // 2.00 QR

        // --- Others / Volatile ---
        { "lbp", 5000000 }, // Highly volatile
        { "afn", 4000 },  // ~؋40.00
        { "azn", 100 },   // ~1.00 ₼
        { "bnd", 100 },   // ~$1.00
        { "khr", 250000 },// ~៛2,500
        { "kgs", 5000 },  // ~50.00 som
        { "kzt", 30000 }, // ~300.00 ₸
        { "lak", 1200000 }, // ~₭12,000
        { "mnt", 200000 },// ~₮2,000
        { "mmk", 150000 },// ~K1,500
        { "pgk", 200 },   // ~K2.00
        { "tjs", 600 },   // ~6.00 som
        { "top", 150 },   // ~T$1.50
        { "uzs", 700000 },// ~7,000 so'm
        { "vuv", 60 },    // ~60 VT (Zero Decimal)
        { "wst", 200 },   // ~WS$2.00
        { "yer", 15000 }, // ~150 ﷼

        // --- Latin America & Caribbean ---
        { "brl", 50 },    // R$0.50
        { "ars", 50000 }, // ~$500.00 (High Inflation)
        { "clp", 500 },   // $500 (Zero Decimal)
        { "cop", 300000 },// $3,000
        { "pen", 200 },   // S/2.00
        { "uyu", 2500 },  // $25.00
        { "bob", 400 },   // Bs 4.00
        { "crc", 30000 }, // ₡300.00
        { "dop", 3000 },  // RD$30.00
        { "gtq", 400 },   // Q4.00
        { "hnl", 1500 },  // L15.00
        { "nio", 2000 },  // C$20.00
        { "pab", 50 },    // B/.0.50
        { "pyg", 4000 },  // ₲4,000 (Zero Decimal)
        { "ang", 100 },   // ƒ1.00
        { "awg", 100 },   // ƒ1.00
        { "bbd", 100 },   // $1.00
        { "bmd", 50 },    // $0.50
        { "bsd", 50 },    // $0.50
        { "bzd", 100 },   // $1.00
        { "fjd", 150 },   // $1.50
        { "gyd", 12000 }, // $120.00
        { "htg", 7500 },  // G75.00
        { "jmd", 10000 }, // $100.00
        { "kyd", 50 },    // $0.50
        { "srd", 2000 },  // $20.00
        { "ttd", 400 },   // $4.00
        { "xcd", 150 },   // $1.50

        // --- Africa ---
        { "zar", 1000 },  // R10.00
        { "egp", 3000 },  // E£30.00
        { "ngn", 80000 }, // ₦800.00
        { "kes", 8000 },  // KSh 80.00
        { "mad", 600 },   // 6.00 dh
        { "tzs", 150000 },// 1,500 TSh
        { "ugx", 2500 },  // 2,500 USh (Zero Decimal)
        { "aoa", 50000 }, // 500.00 Kz
        { "bif", 2000 },  // 2,000 FBu (Zero Decimal)
        { "bwp", 800 },   // P8.00
        { "cdf", 150000 },// 1,500.00 FC
        { "cve", 6000 },  // 60.00 Esc
        { "djf", 100 },   // 100 Fdj (Zero Decimal)
        { "dzd", 8000 },  // 80.00 DA
        { "etb", 3500 },  // 35.00 Br
        { "gmd", 4000 },  // D40.00
        { "gnf", 5000 },  // 5,000 FG (Zero Decimal)
        { "lsl", 1000 },  // 10.00 L
        { "lrd", 10000 }, // $100.00
        { "mga", 2500 },  // 2,500 Ar (Zero Decimal)
        { "mro", 2000 },  // 20.00 MRU
        { "mur", 3000 },  // Rs 30.00
        { "mwk", 60000 }, // 600.00 MK
        { "mzn", 4000 },  // 40.00 MT
        { "nad", 1000 },  // $10.00
        { "rwf", 700 },   // 700 RF (Zero Decimal)
        { "scr", 1000 },  // 10.00 SR
        { "sll", 150000 },// 1,500.00 Le
        { "sos", 30000 }, // 300.00 S
        { "std", 1500 },  // 15.00 Db
        { "szl", 1000 },  // 10.00 L
        { "xaf", 400 },   // 400 FCFA (Zero Decimal)
        { "xof", 400 },   // 400 CFA (Zero Decimal)
        { "zmw", 1500 },  // 15.00 ZK

        // --- Others / Special ---
        { "try", 2000 },  // ₺20.00
        { "rub", 6000 },  // ₽60.00
        { "xpf", 60 }     // 60 F (Zero Decimal)
    }.ToFrozenDictionary();
}
