namespace Optique.Infrastructure.Utils;

public static class AmountToWordsFr
{
    public static string ToDirhams(decimal amount)
    {
        amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        var abs = amount < 0 ? -amount : amount;

        long dh = (long)decimal.Truncate(abs);
        int ct = (int)decimal.Round((abs - dh) * 100m, 0, MidpointRounding.AwayFromZero);
        if (ct == 100) { dh += 1; ct = 0; }

        var dhTxt = ToFrench(dh);
        var dhUnit = dh <= 1 ? "dirham" : "dirhams";

        if (ct == 0) return $"{dhTxt} {dhUnit}";
        var ctTxt = ToFrench(ct);
        var ctUnit = ct <= 1 ? "centime" : "centimes";
        return $"{dhTxt} {dhUnit} et {ctTxt} {ctUnit}";
    }

    // ----------- nombres en lettres (0 .. milliards) -----------
    public static string ToFrench(long n)
    {
        if (n == 0) return "zéro";
        if (n < 0) return "moins " + ToFrench(-n);

        return Compose(n).Trim();
    }

    private static string Compose(long n)
    {
        if (n >= 1_000_000_000)
        {
            long mld = n / 1_000_000_000; long r = n % 1_000_000_000;
            var head = Compose(mld) + " " + (mld == 1 ? "milliard" : "milliards");
            return r == 0 ? head : head + " " + Compose(r);
        }
        if (n >= 1_000_000)
        {
            long m = n / 1_000_000; long r = n % 1_000_000;
            var head = Compose(m) + " " + (m == 1 ? "million" : "millions");
            return r == 0 ? head : head + " " + Compose(r);
        }
        if (n >= 1_000)
        {
            long k = n / 1_000; long r = n % 1_000;
            var head = k == 1 ? "mille" : Compose(k) + " mille";
            return r == 0 ? head : head + " " + Under1000(r);
        }
        return Under1000(n);
    }

    private static string Under1000(long n)
    {
        long c = n / 100; long r = n % 100;
        var parts = new List<string>();

        if (c > 0)
        {
            if (c == 1) parts.Add("cent");
            else
            {
                parts.Add(Unit(c));
                parts.Add(r == 0 ? "cents" : "cent");   // « cents » seulement si rien après
            }
        }
        if (r > 0) parts.Add(Under100(r));

        return string.Join(" ", parts);
    }

    private static string Under100(long n)
    {
        if (n < 17) return Unit(n);
        if (n < 20) return "dix-" + Unit(n - 10);

        long d = n / 10; long u = n % 10;

        switch (d)
        {
            case 2: // 20
                return u == 1 ? "vingt et un" : Join("vingt", u);
            case 3: // 30
                return u == 1 ? "trente et un" : Join("trente", u);
            case 4: // 40
                return u == 1 ? "quarante et un" : Join("quarante", u);
            case 5: // 50
                return u == 1 ? "cinquante et un" : Join("cinquante", u);
            case 6: // 60
                return u == 1 ? "soixante et un" : Join("soixante", u);
            case 7: // 70 = 60 + 10..19
            {
                if (u == 1) return "soixante et onze";
                return "soixante-" + (u == 0 ? "dix" : Under20(u + 10));
            }
            case 8: // 80 = quatre-vingts (s si seul), 81..89
            {
                var base80 = "quatre-vingt" + (u == 0 ? "s" : "");
                return u == 0 ? base80 : base80 + "-" + Unit(u);
            }
            case 9: // 90 = 80 + 10..19 (jamais « et »)
                return "quatre-vingt-" + (u == 0 ? "dix" : Under20(u + 10));
            default:
                return Unit(n);
        }
    }

    private static string Under20(long n)
    {
        if (n < 17) return Unit(n);
        return n switch
        {
            17 => "dix-sept",
            18 => "dix-huit",
            19 => "dix-neuf",
            _ => Unit(n)
        };
    }

    private static string Join(string tens, long u)
        => u == 0 ? tens : tens + "-" + Unit(u);

    private static string Unit(long n) => n switch
    {
        0 => "zéro",
        1 => "un",
        2 => "deux",
        3 => "trois",
        4 => "quatre",
        5 => "cinq",
        6 => "six",
        7 => "sept",
        8 => "huit",
        9 => "neuf",
        10 => "dix",
        11 => "onze",
        12 => "douze",
        13 => "treize",
        14 => "quatorze",
        15 => "quinze",
        16 => "seize",
        _ => ""
    };
}
