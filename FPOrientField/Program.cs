using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace FPOrientField{
    internal enum ExtractorResult{
        Success = 0,
        SyntaxError = 1,
        CannotOpenImageFile = 2,
        CannotOpenForegroundFile = 3,
        CannotOpenPipe = 4,
        CannotSaveOrientations = 5,
    };

    class Program{
        private const string PipeName = "PipeForFVCOrientation";

        static int Main(string[] args){
            // Check input parameters
            if (args.Length != 2){
                Console.WriteLine("Syntax error.\nUse: Extractor <index file> <output folder>");
                return (int) ExtractorResult.SyntaxError;
            }

            // Manages the index file
            var lines = File.ReadAllLines(args[0]);
            int imagesCount;
            if (!int.TryParse(lines[0], out imagesCount)){
                Console.WriteLine("Index file format error.");
                return (int) ExtractorResult.SyntaxError;
            }

            // Output path
            var outputPath = args[1];
            if (!Directory.Exists(outputPath)){
                Console.WriteLine("Output path not found.");
                return (int) ExtractorResult.SyntaxError;
            }

            // connects to the named pipe
            // Note: during debugging you may comment out pipe-related code to avoid having to open the pipe during debugging
//            using (var outputPipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out)) {
//                try {
//                    outputPipeClient.Connect(10000);
//                }
//                catch (Exception ex) {
//                    Console.WriteLine("Cannot open pipe: {0}", ex.Message);
//                    return (int)ExtractorResult.CannotOpenPipe;
//                }
//   
//                using (var sw = new StreamWriter(outputPipeClient)){
//                   sw.AutoFlush = true;
    
                    // Executes the algorithm on all the images
                    for (var i = 1; i <= imagesCount; i++){
                        // Parses the index line
                        var fields = lines[i].Split(' ');
                        if (fields.Length != 3){
                            Console.WriteLine("Index line format error.");
                            return (int) ExtractorResult.SyntaxError;
                        }
        
                        var imagePath = fields[0];
                        if (!File.Exists(imagePath)){
                            return (int) ExtractorResult.CannotOpenImageFile;
                        }
        
                        var step = int.Parse(fields[1]);
                        var border = int.Parse(fields[2]);
        
                        // a foreground file must exists for each image
                        var foregroundPath = Path.ChangeExtension(imagePath, "fg");
                        if (!File.Exists(foregroundPath)){
                            return (int) ExtractorResult.CannotOpenForegroundFile;
                        }
        
                        // reads the foreground file
                        var fgLines = File.ReadAllLines(foregroundPath);
                        var fgHeaderFields = fgLines[0].Split(' ');
                        var fgRows = int.Parse(fgHeaderFields[0]);
                        var fgColumns = int.Parse(fgHeaderFields[1]);
                        var foreground = new bool[fgRows][];
                        for (var y = 0; y < fgRows; y++){
                            foreground[y] = Array.ConvertAll(fgLines[y + 1].Split(' '), s => s == "1");
                        }
        
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                        
                        var g = new Grid(foreground, fgRows, fgColumns, border, step);
                        UtilityFunctions.SetInitialData(imagePath, g);
                        
                        // extracts the orientations
                        var orientations = new byte[fgRows][];
                        for (var y = 0; y < fgRows; y++){
                            orientations[y] = new byte[fgColumns];
                            for (var x = 0; x < fgColumns; x++){
                                if (!foreground[y][x]) continue;
                                var px = border + x * step;
                                var py = border + y * step;
        
                                orientations[y][x] = UtilityFunctions.GetDirectionAtPoint(px, py);
                            }
                        }
                        
                        var interpolator = new Interpolator(orientations, 
                                                            UtilityFunctions.GetQualityMeasure(),
                                                            UtilityFunctions.GetThreshold(),
                                                            foreground, 
                                                            fgRows, 
                                                            fgColumns);
                        interpolator.Interpolate();
                        
                        Console.WriteLine(stopwatch.ElapsedMilliseconds);
                        stopwatch.Stop();
        
                        // Saves the result
                        var filename = Path.GetFileNameWithoutExtension(imagePath);
                        var outputFile = Path.Combine(outputPath, filename + ".dirmap");
                        try{
                            SaveOrientationsToFile(outputFile, orientations, border, step, foreground);
                        }
                        catch (Exception ex){
                            Console.WriteLine("Cannot save orientations: {0}", ex.Message);
                            return (int) ExtractorResult.CannotSaveOrientations;
                        }
        
//                        sw.WriteLine(imagePath);
//                    }
//                }
            }

            return (int) ExtractorResult.Success;
        }

        private static void SaveOrientationsToFile(string path, byte[][] orientations, int border, int step, bool[][] foreground){
            const string fileHeader = "DIRIMG00";
            using (var file = new FileStream(path, FileMode.Create)){
                var rows = orientations.Length;
                var columns = orientations[1].Length;
                var writer = new BinaryWriter(file);
                writer.Write(Encoding.ASCII.GetBytes(fileHeader));
                writer.Write(border); // border x
                writer.Write(border); // border y
                writer.Write(step); // step x
                writer.Write(step); // step y
                writer.Write(columns);
                writer.Write(rows);
                for (var y = 0; y < rows; y++){
                    for (var x = 0; x < columns; x++){
                        writer.Write(orientations[y][x]); // the orientation (stored as a byte)
                        writer.Write((byte) (foreground[y][x]
                            ? 255
                            : 0)); // the strength (not considered in this evaluation)                  
                    }
                }
            }
        }
    }
}