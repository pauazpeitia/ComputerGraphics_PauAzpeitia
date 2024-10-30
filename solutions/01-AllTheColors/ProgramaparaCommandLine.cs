using System;
using CommandLine;

class ProgramaparaCommandLine
{
    // Clase que define los argumentos de la línea de comandos
    public class Options
    {
        [Option('n', "name", Required = true, HelpText = "Su nombre.")]
        public string Name { get; set; }

        [Option('a', "age", Required = false, HelpText = "Su edad, si es tan amable.")]
        public int? Age { get; set; } 
    }

    public void PedirNombre(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(a =>
            {   string ageMessage = a.Age.HasValue? $" tienes {a.Age.Value} años" : "";
                Console.WriteLine($"¡Hola, {a.Name}{ageMessage}! Bienvenido al programa.");
            })
            .WithNotParsed(errors =>
            {
                Console.WriteLine("Error al analizar los argumentos. Asegúrese de proporcionar bien su nombre, y si desea su edad.");
            });
    }
}
