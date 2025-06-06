using Calculator;

if (args.Length != 1)
{
    Console.WriteLine("Usage: compiler <source-file.ks>");
    System.Environment.Exit(1);
}

string filePath = args[0];

if (!Path.GetExtension(filePath).Equals(".ks", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("Error: Source file must have .ks extension");
    System.Environment.Exit(1);
}

if (!File.Exists(filePath))
{
    Console.WriteLine($"Error: File '{filePath}' not found");
    System.Environment.Exit(1);
}

try
{
    string source = File.ReadAllText(filePath);
    RunFile(source);
}
catch (IOException ex)
{
    Console.WriteLine($"Error reading file: {ex.Message}");
    System.Environment.Exit(1);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    System.Environment.Exit(1);
}

static void RunFile(string source)
{
    var lexer = new Lexer(source);
    var tokens = lexer.ScanTokens();
    
    var parser = new Parser(tokens);
    var statements = parser.Parse();
    
    var interpreter = new Interpreter();
    interpreter.Interpret(statements);
}
