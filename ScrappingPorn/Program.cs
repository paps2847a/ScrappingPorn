using System.IO;
using System.Text;

namespace ScrappingPorn
{
    internal class Program
    {
        public static Action<Object> Print = (Object x) => Console.WriteLine(x);
        static async Task Main(string[] args)
        {
            try
            {
                var imgControl = new ImageDownloader(new PathHandler());
                Print("Bienvenido animal de mierda que deseas hacer?");
                Print("");
                Print("");
                Print("Opciones:");
                Print("");

                while (true)
                {
                    Print("1) Establecer direccion de guardado de imagenes");
                    Print("2) Descargar Imagenes");
                    Print("3) Cerrar Programa");
                    Print("");
                    var opcion = int.TryParse(Console.ReadLine(), out int value);
                    if (!opcion)
                    {
                        Print("no es una opcion valida pendejo");
                        continue;
                    }

                    if (value > 3 || value == 0)
                    {
                        Print("No hay opcion con ese numero imbecil");
                        continue;
                    }

                    if (value == 1)
                    {
                        Print("Estas son las opciones de manejo del destino de guardado, que quieres hacer?:");
                        Print("1) Crear Path");
                        Print("2) Ver Path");
                        Print("3) Editar Path");

                        var opcionPath = int.TryParse(Console.ReadLine(), out int valuePath);
                        switch (valuePath)
                        {
                            case 1:
                                if (imgControl.CreatPath())
                                {
                                    Print("Path Creado");
                                    Print("");
                                }
                                else
                                {
                                    Print("La direccion del archivo ya existe o hubo un problema, decifralo tu imbecil");
                                    Print("");
                                }
                                break;
                            case 2:
                                if (imgControl.GetStrPath() == string.Empty)
                                {
                                    Print("O no existe el archivo o no tiene nada dentro, imbecil");
                                    Print("");
                                }
                                else
                                {
                                    Print($"DIRECCION GUARDADA DE PATH: {imgControl.GetStrPath()}");
                                    Print("");
                                }
                                break;
                            case 3:
                                Print("Pon la direccion base en donde quieres crear las carpetas con tus cochinadas, procura poner solo la direccion base:");
                                if (imgControl.SavPath(Console.ReadLine()) != string.Empty)
                                {
                                    Print("Direccion Creada");
                                    Print("");
                                }
                                else
                                {
                                    Print("Hubo un error");
                                    Print("");
                                }
                                break;
                            default:
                                Print("No existe tal opcion");
                                break;
                        }
                    }

                    if (value == 2)
                    {
                        Print("Hora de la accion nena");
                        Print("Pon la URL del sitio, el nombre de la carpeta y el rango de imagenes que quieres. Todo separado por una coma ','");
                        var rawData = Console.ReadLine()?.Split(",");
                        try
                        {
                            if (rawData == null || rawData.Length != 4)
                                throw new Exception("Problema al insertar los datos");

                            if (rawData[1][rawData[1].Length - 1] != '/')
                                throw new Exception("Te hace falta el / a lo ultimo de la direccion URL");

                            var polnoResult = await imgControl.DownlaodImgs(rawData[0], rawData[1], int.Parse(rawData[2]), int.Parse(rawData[3]));
                            if (polnoResult)
                            {
                                Print("");
                                Print("Hora de la paja");
                                Print("");
                            }
                            else
                            {
                                Print("");
                                Print("Hubo un problema...");
                                Print("Noooo mi polnoooooo");
                                Print("");
                            }
                        }
                        catch (Exception e)
                        {
                            Print(e.Message);
                        }
                    }

                    if (value == 3)
                    {
                        Print("Gracias por hacerme perder mi tiempo imbecil");
                        Print("Presiona cualquier tecla para cerrar");
                        Console.ReadLine();
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                ex.InformError();
            }
        }
    }

    public class ImageDownloader
    {
        private readonly HttpClient _client;
        private IPathHandler _pathHandler;

        public ImageDownloader(IPathHandler pathHandler)
        {
            _client = new HttpClient();
            _pathHandler = pathHandler;
        }

        public string GetStrPath() => _pathHandler.GetPath();

        public string SavPath(string? path)
        {
            try
            {
               var r = _pathHandler.SavePath(path);
               return r;
            }
            catch(Exception)
            {
                return string.Empty;
            }
        }

        public bool CreatPath()
        {
            try
            {
                return _pathHandler.CreatePathFile();
            }
            catch(Exception)
            {
                return false;
            }
        }


        public async Task<bool> DownlaodImgs(string urlToDownload, string carpeta, int minRange, int maxRange)
        {
            try
            {
                //var urlBase = "https://m9.imhentai.xxx/028/ox1ve25iar/";
                var descargas = new List<Task>();
                if(minRange == 0 || maxRange == 0)
                    return false;

                if (_pathHandler.GetPath() == string.Empty || _pathHandler.GetPath() == null)
                    return false;

                if (!Directory.Exists(@$"{_pathHandler.GetPath()}\{carpeta}"))
                    Directory.CreateDirectory(@$"{_pathHandler.GetPath()}\{carpeta}");

                var builder = new StringBuilder(@$"{_pathHandler.GetPath()}\{carpeta}");
                for (var init = minRange; init <= maxRange; ++init)
                {
                    try
                    {
                        var rawData = await _client.GetStreamAsync(@$"{urlToDownload}{init}.jpg");
                        builder.Append(@$"\{init}.jpg");
                        using (var filestream = new FileStream(builder.ToString(), FileMode.Create, FileAccess.Write))
                        {
                            descargas.Add(rawData.CopyToAsync(filestream));
                        }

                        builder.RemoveLastImage();
                    }
                    catch(Exception ex)
                    {
                        ex.InformError();
                        continue;
                    }
                }

                await Task.WhenAll(descargas);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        } 
    }

    public interface IPathHandler
    {
        string SavePath(string? path);
        bool CreatePathFile();
        string GetPath();
    }

    public class PathHandler : IPathHandler
    {
        private string AppContx = @$"{AppContext.BaseDirectory}FilePATH\direction.txt";

        public bool CreatePathFile()
        {
            try
            {
                if (!Directory.Exists(@$"{AppContext.BaseDirectory}FilePATH"))
                    Directory.CreateDirectory(@$"{AppContext.BaseDirectory}FilePATH");

                if (File.Exists(AppContx))
                {
                    return false;
                }
                else
                {
                    using (var file = new FileStream(AppContx, FileMode.CreateNew, FileAccess.Write))
                    {
                        file.Write(Encoding.UTF8.GetBytes("aqui se sobreescribira con la direccion de guardado que quieras"));
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string SavePath(string? path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return string.Empty;

                if (File.Exists(AppContx))
                {
                    File.WriteAllText(AppContx, path);
                }
                else
                {
                    return "No existe el archivo path";
                }

                return "exito";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetPath()
        {
            try
            {
                return File.ReadAllText(AppContx);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }

    public static class ExtensionFunctionsStringBuilder
    {
        public static void RemoveLastImage(this StringBuilder builder)
        {
            var result = builder.ToString().Split(@"\").ToList().SkipLast(1);
            builder.Clear().Append(String.Join(@"\", result));
        }
    }

    public static class ErrorInformerExtension
    {
        private static string PathError = $@"{AppContext.BaseDirectory}ErrorInformer";

        public static void InformError(this Exception ex)
        {
            if (!Directory.Exists(PathError))
                Directory.CreateDirectory(PathError);

            var fail = new FileStream($@"{PathError}/{DateTime.Now.ToString("yyyy-MM-dd")}-id:{Guid.NewGuid().ToString()}-error.txt", FileMode.CreateNew, FileAccess.Write);
            fail.Write(Encoding.UTF8.GetBytes($@"Error: {ex.Message}"));
            fail.Flush();
        }
    }
}
