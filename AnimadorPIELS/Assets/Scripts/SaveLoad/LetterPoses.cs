using NaughtyAttributes;

public static class LetterPoses
{
    public static readonly string A = "A";
    public static readonly string B = "B";
    public static readonly string C = "C";
    public static readonly string CH = "CH";
    public static readonly string D = "D";
    public static readonly string E = "E";
    public static readonly string F = "F";
    public static readonly string G = "G";
    public static readonly string H = "H";
    public static readonly string I = "I";
    public static readonly string J = "J";
    public static readonly string K = "K";
    public static readonly string L = "L";
    public static readonly string M = "M";
    public static readonly string N = "N";
    public static readonly string O = "O";
    public static readonly string P = "P";
    public static readonly string Q = "Q";
    public static readonly string R = "R";
    public static readonly string S = "S";
    public static readonly string T = "T";
    public static readonly string U = "U";
    public static readonly string V = "V";
    public static readonly string W = "W";
    public static readonly string X = "X";
    public static readonly string Y = "Y";
    public static readonly string Z = "Z";

    public static DropdownList<string> GetLettersPoseName()
    {
        return new DropdownList<string>()
        {
            { "A", A },
            { "B", B },
            { "C", C },
            { "CH", CH },
            { "D", D },
            { "E", E },
            { "F", F },
            { "G", G },
            { "H", H },
            { "I", I },
            { "J", J },
            { "K", K },
            { "L", L },
            { "M", M },
            { "N", N },
            { "O", O },
            { "P", P },
            { "Q", Q },
            { "R", R },
            { "S", S },
            { "T", T },
            { "U", U },
            { "V", V },
            { "W", W },
            { "X", X },
            { "Y", Y },
            { "Z", Z },
        };
    }
}
