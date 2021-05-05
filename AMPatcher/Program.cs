using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnpatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using System.Threading;
using System.IO;
using Spectre.Console.Rendering;

namespace AMPatcher
{
    class Program
    {
        private static Target FN_GetSoftwareEdition;
        private static Target CLS_LicenseManager;
        private static Target FN_ValidateRegistrationCode;
        private static Target FN_IsRegisteredFromBackgroundThread;

        static void Main(string[] args)
        {
            GradientMarkup.DefaultStartColor = "#ed4264";
            GradientMarkup.DefaultEndColor = "#ffedbc";
            AnsiConsole.Markup(GradientMarkup.Ascii("Awesome Miner", FigletFont.Default, "#009245", "#FCEE21"));
            AnsiConsole.Markup(GradientMarkup.Text("Awesome Miner v8.4.x Keygen by Ankur Mathur"));

            var filePath = Path.Combine(Environment.CurrentDirectory, "AwesomeMiner.exe");
            if (!File.Exists(filePath))
            {
                AnsiConsole.MarkupLine($"\r\n[bold red]File not found in the current directory ![/] ({filePath})");
                Console.ReadKey();
                Environment.Exit(0);
            }

            var p = new Patcher(filePath);

            var licMgr = p.FindMethodsByOpCodeSignature(
                OpCodes.Call,
                OpCodes.Ldarg_1,
                OpCodes.Stfld);
            
            foreach (var item in licMgr)
            {
                var ins = p.GetInstructions(item);
                var operands = ins.Where(x => x.OpCode == OpCodes.Stfld).Select(x => x.Operand).OfType<dnlib.DotNet.FieldDef>();
                if (operands.Any(x => x.Name == "RegistrationSettings"))
                {
                    item.Method = null;
                    CLS_LicenseManager = item;
                    AnsiConsole.Markup(GradientMarkup.Text($"Found LicenseManager Class at : {CLS_LicenseManager.Class}"));
                    break;
                }
            }

            var targets = p.FindMethodsByOpCodeSignature(
                OpCodes.Ldc_I4_1,
                OpCodes.Newarr,
                OpCodes.Stloc_0,
                OpCodes.Ldloc_0,
                OpCodes.Ldc_I4_0,
                OpCodes.Ldarg_0,
                OpCodes.Stelem_Ref,
                OpCodes.Call,
                OpCodes.Call,
                OpCodes.Ldstr,
                OpCodes.Ldloc_0,
                OpCodes.Call,
                OpCodes.Unbox,
                OpCodes.Ldobj,
                OpCodes.Ret).Where(t => t.Class.Equals(CLS_LicenseManager.Class)).ToArray();

            FN_GetSoftwareEdition = targets.FirstOrDefault(x => p.FindInstruction(x, Instruction.Create(OpCodes.Unbox, new TypeDefUser("AwesomeMiner.Components.SoftwareEdition"))) > -1);
            if (FN_GetSoftwareEdition != null)
            {
                AnsiConsole.Markup(GradientMarkup.Text($"GetSoftwareEdition Method at : {FN_GetSoftwareEdition.Method} ... "));

                FN_GetSoftwareEdition.Instructions = new Instruction[]
                {
                        Instruction.Create(OpCodes.Ldc_I4, 10),
                        Instruction.Create(OpCodes.Ret)
                };

                p.Patch(FN_GetSoftwareEdition);
                AnsiConsole.Markup("[Green]Success[/]");
            }

            targets = p.FindMethodsByArgumentSignatureExact(CLS_LicenseManager, null, "AwesomeMiner.Entities.Settings.RegistrationSettings", "System.Boolean", "System.String");

            FN_ValidateRegistrationCode = targets.FirstOrDefault();
            if (FN_ValidateRegistrationCode != null)
            {
                AnsiConsole.Markup(GradientMarkup.Text($"ValidateRegistrationCode Method at : {FN_ValidateRegistrationCode.Method} ... "));

                FN_ValidateRegistrationCode.Instructions = new Instruction[]
                {
                        Instruction.Create(OpCodes.Ldc_I4_0),
                        Instruction.Create(OpCodes.Ret)
                };

                p.Patch(FN_ValidateRegistrationCode);

                AnsiConsole.MarkupLine("[Green]Success[/]");
            }

            targets = p.FindMethodsByArgumentSignatureExact(CLS_LicenseManager, "System.Boolean", "System.Boolean");

            FN_IsRegisteredFromBackgroundThread = targets.FirstOrDefault();
            if (FN_IsRegisteredFromBackgroundThread != null)
            {
                AnsiConsole.Markup(GradientMarkup.Text($"IsRegisteredFromBackgroundThread Method at : {FN_IsRegisteredFromBackgroundThread.Method} ... "));

                p.WriteReturnBody(FN_IsRegisteredFromBackgroundThread, true);

                AnsiConsole.MarkupLine("[Green]Success[/]");
            }

            AnsiConsole.Markup(GradientMarkup.Text("Saving patched file... "));
            p.Save(true);
            AnsiConsole.MarkupLine("[Green]Success[/]");
            
            AnsiConsole.MarkupLine($"[bold]Edition: [yellow]UltimatePlus[/][/]");
            var email = AnsiConsole.Ask<string>("Enter [green]email address[/] to register");
            AnsiConsole.MarkupLine($"[bold red on silver]Serial Number: {GenerateSerial("UltimatePlus", email)}[/]");
            
            Console.ReadKey();
        }

        private static string GenerateSerial(string ed, string email)
        {
            var editionStr = "S";
            switch (ed)
            {
                case "Standard":
                    editionStr = "S";
                    break;
                case "Professional":
                    editionStr = "P";
                    break;
                case "Premium":
                    editionStr = "R";
                    break;
                case "Enterprise":
                    editionStr = "E";
                    break;
                case "Enterprise60":
                    editionStr = "N";
                    break;
                case "Ultimate100":
                    editionStr = "L01";
                    break;
                case "Ultimate200":
                    editionStr = "L02";
                    break;
                case "Ultimate300":
                    editionStr = "L03";
                    break;
                case "UltimatePlus":
                    editionStr = "T9999";
                    break;
                case "ProductSubscription":
                    editionStr = "XP";
                    break;
                default:
                    break;
            }

            var hash = System.Security.Cryptography.SHA1.Create().ComputeHash(UTF8Encoding.UTF8.GetBytes($"{editionStr}{email}SuchSecure"));
            var hashStr = string.Empty;
            hash.ToList().ForEach(h => hashStr += string.Format("{0:X2}", h));
            return $"{editionStr}{hashStr.Substring(0, 17 - editionStr.Length)}";
        }

    }
}
