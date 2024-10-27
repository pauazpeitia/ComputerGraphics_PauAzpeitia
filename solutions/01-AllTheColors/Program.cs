using System;
using CommandLine;

class Program
{
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(o =>
            {
                Console.WriteLine($"¡Hola, {o.Name}! Bienvenido al programa.");
            })
            .WithNotParsed(errors =>
            {
                Console.WriteLine("Error al analizar los argumentos. Asegúrese de proporcionar su nombre.");
            });
    }
}

