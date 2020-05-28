using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using testOcr2.Models;
using Tesseract;

using System.IO;
using System.Reflection;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Text;
using Microsoft.AspNetCore.Http;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using Microsoft.AspNetCore.Hosting;


namespace testOcr2.Controllers
{
    public class HomeController : Controller
    {
        public const string folderName = "images/";
        public const string trainedDataFolderName = "./tessdata";
        public const string  docPath = @"D:\\applications\\testOcr2\\";
     
        private IHostingEnvironment _hostingEnvironment;

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(UploadModel model)
        {
            var img = model.FormFile;
            var imgCaption = model.ImageCaption;
            string getTextLink=this.Upload(model.FormFile);
            //Getting file meta data
            var fileName = Path.GetFileName(model.FormFile.FileName);
            string varText = this.ExtractTextFromImage(getTextLink);
            ViewData["text_read"] = varText;
            this.PdfConvertAsync(varText, fileName);
            string docPath = fileName;
            ViewData["pdf_link"] = docPath;
            ViewData["Message"] = "Your application description page.";
           
            return View();
           
        }
        
        [HttpGet]
        public ActionResult DownloadDocument(string file_name)
        {

           string fileName = file_name+".pdf";

           string files = System.IO.Path.Combine("pdf/") + fileName;
            if (!System.IO.File.Exists(files))
            {
                return RedirectToAction("index", new { message = "File Not Found" });

            }
            else
            {

                byte[] fileBytes = System.IO.File.ReadAllBytes(files);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, files);

            }


        }



      
        public async Task PdfConvertAsync(string Text_line,string File_name) {

            
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "WriteTextAsync.txt")))
            {
                await outputFile.WriteAsync(Text_line);
            }


         try
            {
            string line = null;

         System.IO.TextReader readFile = new StreamReader(docPath+ "WriteTextAsync.txt");
            int yPoint = 0;

                PdfDocument pdf = new PdfDocument();
                pdf.Info.Title = "TXT to PDF";
                PdfPage pdfPage = pdf.AddPage();
                XGraphics graph = XGraphics.FromPdfPage(pdfPage);
                XFont font = new XFont("Verdana", 20, XFontStyle.Regular);

                while (true)
                {
                    line = readFile.ReadLine();
                if (line == null)
                    {
                        break; 
                    }
                    else
                    {
                        graph.DrawString(line, font, XBrushes.Black, new XRect(40, yPoint, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
                        yPoint = yPoint + 40;
                    }
                }
             
                string pdfFilename = "pdf/"+ File_name + ".pdf";
                if (System.IO.File.Exists( pdfFilename))
                {
                    System.IO.File.Delete( pdfFilename);
                }
                pdf.Save(pdfFilename);
                readFile.Close();
                readFile = null;
                Process.Start(pdfFilename);
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.ToString());
            }

        }

       


        public String ExtractTextFromImage(string file_lnk)
        {
           var IMage_file = file_lnk;
            string name = Path.GetFileName(IMage_file);
            
            string sourceFile = System.IO.Path.Combine(IMage_file, name);
            string destFile = System.IO.Path.Combine(folderName, name);


           
            var image = IMage_file;

           
    
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            path = Path.Combine(path, "tessdata");
            path = path.Replace("file:\\", "");

            string tessPath = Path.Combine(trainedDataFolderName, "");
            string result = "";

            using (var engine = new TesseractEngine(path, "eng", EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(IMage_file))
                {
                    var page = engine.Process(img);
                    result = page.GetText();
                    Console.WriteLine(result);
                }
            }
            return String.IsNullOrWhiteSpace(result) ? "Ocr is finished. Return empty" : result;



        }

        public string Upload(IFormFile file)
        {
            // Extract file name from whatever was posted by browser
            var fileName = System.IO.Path.GetFileName(file.FileName);

            // If file with same name exists delete it
            if (System.IO.File.Exists("img/"+fileName))
            {
                System.IO.File.Delete("img/" + fileName);
            }
            string Final_location_name = "";
            string destFile = System.IO.Path.Combine("img/", fileName);
            // Create new local file and copy contents of uploaded file
            using (var localFile = System.IO.File.OpenWrite(destFile))
            using (var uploadedFile = file.OpenReadStream())
            {
                uploadedFile.CopyTo(localFile);
                Final_location_name = localFile.Name;
            }

            

            return Final_location_name;
        }

       



        
       

        public async Task<IActionResult> OnPostUploadAsync(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var filePath = Path.GetTempFileName();

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(new { count = files.Count, size });
        }

        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
