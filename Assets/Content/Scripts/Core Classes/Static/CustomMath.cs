public static class CustomMath
{
    public static int Factorial(int n)
    {
        if (n <= 1)
            return 1;
        int result = n;
        for (int i = 1; i < n; i++)
            result *= i;
        return result;
    }
    public static int nCr(int n, int r)
    {
        // https://www.calculatorsoup.com/calculators/discretemathematics/combinations.php
        return Factorial(n) / (Factorial(r) * Factorial(n - r));
    }
}
