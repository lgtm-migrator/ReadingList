open System.IO

open System
open System.Text
open System.Text.RegularExpressions
open System.Globalization
open System.Diagnostics

// white space, em-dash, en-dash, underscore
let private wordDelimiters = new Regex(@"[\s—–_]", RegexOptions.Compiled)
// characters that are not valid
let private invalidChars = new Regex(@"[^a-z0-9\-]", RegexOptions.Compiled)
// multiple hyphens
let private multipleHyphens = new Regex(@"-{2,}", RegexOptions.Compiled)

let private removeDiacritics (stIn : string) = 
    let stFormD = stIn.Normalize(NormalizationForm.FormD)
    let sb = new StringBuilder()
    stFormD |> Seq.iter (fun c -> 
                    let uc = CharUnicodeInfo.GetUnicodeCategory(c)
                    if uc <> UnicodeCategory.NonSpacingMark then sb.Append(c) |> ignore)
    sb.ToString().Normalize(NormalizationForm.FormC)

let slugify (value : string) = 
    value.ToLowerInvariant() // convert to lower case
    |> removeDiacritics // remove diacritics (accents)
    |> fun s -> wordDelimiters.Replace(s, "-") // ensure all word delimiters are hyphens
    |> fun s -> invalidChars.Replace(s, "") // strip out invalid characters
    |> fun s -> multipleHyphens.Replace(s, "-") // replace multiple hyphens (-) with a single hyphen
    |> fun s -> s.Trim('-') // trim hyphens (-) from ends

let downloadImageToSite imageUrl isbn  imagesFolder = 
    let imageName = sprintf "%s%s" isbn (Path.GetExtension(imageUrl))
    let imagePath = sprintf "%s%s" imagesFolder imageName
    File.WriteAllBytes(imagePath, (new System.Net.WebClient()).DownloadData(imageUrl))
    imageName

let createFolderIfNotExists folderPath =
    if not (Directory.Exists folderPath) then
        (Directory.CreateDirectory folderPath) |> ignore

let private cprintf c message = 
    Printf.kprintf 
        (fun s -> 
            let old = System.Console.ForegroundColor 
            try 
                System.Console.ForegroundColor <- c;
                System.Console.Write s
            finally
                System.Console.ForegroundColor <- old) 
        "%s" message

let cprintfn c message = 
    cprintf c message
    printfn ""

let execProcess processName arguments =
    printfn "execute: %s %s" processName arguments

    use proc = new Process()
    proc.StartInfo.UseShellExecute <- false
    proc.StartInfo.FileName <- processName
    proc.StartInfo.Arguments <- arguments
    proc.StartInfo.RedirectStandardOutput <- true
    proc.StartInfo.RedirectStandardError <- true
    proc.ErrorDataReceived.Add(fun d -> 
        if not (isNull d.Data) then eprintfn "%s" d.Data)
    proc.OutputDataReceived.Add(fun d -> 
        if not (isNull d.Data) then printfn "%s" d.Data)
    proc.Start() |> ignore
    proc.BeginErrorReadLine()
    proc.BeginOutputReadLine()
    proc.WaitForExit()
    proc.ExitCode

let execProcessWithFail processName arguments =
    if execProcess processName arguments > 0 then
        failwith ("'" + processName + " " + arguments + "' failed")

type TaskResult =
    | Ok
    | Stop of string