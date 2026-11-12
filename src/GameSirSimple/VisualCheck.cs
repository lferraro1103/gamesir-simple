namespace GameSirSimple;
static class VisualCheck
{
    public static void Run(string[] args)
    {
        // Offline drawing and geometry checks: no windows, SDL, USB or virtual gamepads.
        string folder=args.Length>1?args[1]:AppContext.BaseDirectory;Directory.CreateDirectory(folder);
        var result=new List<string>();
        try {
            if(ControllerView.Spots.Select(s=>s.Index).Distinct().Count()!=25)throw new Exception("Missing controls");
            foreach(var size in new[]{new Size(900,500),new Size(700,320),new Size(1000,650)}){
                var viewport=ControllerView.Viewport(size);
                foreach(var s in ControllerView.Spots){
                    var center=new Point((int)Math.Round(viewport.X+(s.Bounds.X+s.Bounds.Width/2)*viewport.Width/900),(int)Math.Round(viewport.Y+(s.Bounds.Y+s.Bounds.Height/2)*viewport.Height/500));
                    if(ControllerView.HitTest(size,center)!=s.Index)throw new Exception("Hit area mismatch: "+Engine.Names[s.Index]);
                }
                if(ControllerView.HitTest(size,new Point(0,0))!=-1)throw new Exception("Background should not remap");
            }
            result.Add("PASS: 25 unique controls clickable at three window sizes; background ignored.");
            foreach(bool pressed in new[]{false,true}){
                using var b=new Bitmap(1080,600);using var g=Graphics.FromImage(b);g.Clear(Color.FromArgb(248,250,253));var values=new float[25];
                if(pressed){values[0]=1;values[9]=1;values[11]=1;values[16]=1;values[18]=1;}
                ControllerView.Render(g,b.Size,values,pressed?.8f:0,pressed?-.4f:0,0,0,10,14,pressed?"X":null);
                b.Save(Path.Combine(folder,pressed?"joystick-activo.png":"joystick.png"));
            }
            result.Add("PASS: idle and active drawings rendered without opening windows.");
            using var stream=typeof(VisualCheck).Assembly.GetManifestResourceStream("GameSirSimple.app.ico")??throw new Exception("Missing icon");using var icon=new Icon(stream);if(icon.Width<16)throw new Exception("Invalid icon");
            result.Add("PASS: application icon embedded and readable.");
        }catch(Exception ex){Environment.ExitCode=1;result.Add("FAIL: "+ex);}
        File.WriteAllLines(Path.Combine(folder,"visual-check.txt"),result);
    }
}
