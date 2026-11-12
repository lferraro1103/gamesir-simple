namespace GameSirSimple;
static class VisualCheck
{
    sealed class ClickableView:ControllerView{
        public void ClickAt(Point p)=>OnMouseClick(new MouseEventArgs(MouseButtons.Left,1,p.X,p.Y,0));
    }
    static IEnumerable<Control> Descendants(Control c){yield return c;foreach(Control child in c.Controls)foreach(var descendant in Descendants(child))yield return descendant;}
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
            using(var clickable=new ClickableView{Size=new Size(900,500)}){
                string? assigned=null;clickable.AssignRequested+=n=>assigned=n;
                foreach(var s in ControllerView.Spots){assigned=null;clickable.ClickAt(Point.Round(new(s.Bounds.X+s.Bounds.Width/2,s.Bounds.Y+s.Bounds.Height/2)));if(assigned!=Engine.Names[s.Index])throw new Exception("Wrong assignment event: "+s.Index);}
            }
            result.Add("PASS: all 25 click events request the expected button or direction.");
            if(Engine.Shape(.03f,.02f,.1f,1,1)!=(0f,0f))throw new Exception("Dead zone regression");
            if(Math.Abs(Engine.Shape(1,0,.1f,1,1).x-1)>.001)throw new Exception("Full travel regression");
            var diagonal=Engine.Shape(1,1,.1f,1,1);if(diagonal.x*diagonal.x+diagonal.y*diagonal.y>1.001)throw new Exception("Diagonal overflow");
            if(Engine.Shape(.5f,0,.1f,1,2).x>=Engine.Shape(.5f,0,.1f,1,1).x)throw new Exception("Response curve regression");
            var trigger=new Binding("axis",0,1,-1);if(trigger.AxisValue(-1)!=0 || trigger.AxisValue(1)!=1)throw new Exception("Trigger range regression");
            var inverse=new Binding("axis",0,-1,0);if(inverse.AxisValue(-1)!=1 || inverse.AxisValue(1)!=0)throw new Exception("Axis inversion regression");
            result.Add("PASS: radial dead zone, travel, diagonal clamp, response curve and axis/trigger mapping.");
            var settings=new Settings{Language="zh-CN",LeftDead=-5,RightDead=500,Outer=0,Response=900,Centers=new[]{float.NaN,1f,.05f,0},Bindings=new(){["A"]=null!,["B"]=new("axis",0,int.MinValue,0),["X"]=new("button",3,1,0),["unknown"]=new("button",0,1,0)}};
            settings.Normalize();
            if(settings.LeftDead!=0 || settings.RightDead!=40 || settings.Outer!=60 || settings.Response!=200 || settings.Centers[0]!=0 || settings.Centers[1]!=0 || settings.Centers[2]!=.05f || settings.Bindings.Count!=1 || !settings.Bindings.ContainsKey("X"))throw new Exception("Invalid settings not repaired");
            var restored=System.Text.Json.JsonSerializer.Deserialize<Settings>(System.Text.Json.JsonSerializer.Serialize(settings))!;restored.Normalize();if(restored.Bindings["X"].Index!=3 || restored.Language!="zh-CN")throw new Exception("Saved binding/language roundtrip failed");
            restored.Language="invalid";restored.Normalize();if(restored.Language!="es")throw new Exception("Invalid language fallback failed");
            using(var engine=new Engine(new Settings(),false)){engine.Values[0]=1;engine.LX=.8f;engine.ClearInput();if(engine.Values.Any(v=>v!=0) || engine.LX!=0)throw new Exception("Stale input not cleared");}
            result.Add("PASS: malformed settings repaired, valid bindings preserved, saved schema roundtrip and input reset.");
            string embedded=System.Text.Encoding.UTF8.GetString(SDL.EmbeddedMapping());if(!embedded.Contains(",GameSir X5 Lite,") || !embedded.Contains("platform:Windows,"))throw new Exception("Missing self-contained model mapping");
            result.Add("PASS: X5 Lite mapping embedded; no external mapping file needed.");
            foreach(bool pressed in new[]{false,true}){
                using var b=new Bitmap(1080,600);using var g=Graphics.FromImage(b);g.Clear(Color.FromArgb(248,250,253));var values=new float[25];
                if(pressed){values[0]=1;values[9]=1;values[11]=1;values[16]=1;values[18]=1;}
                ControllerView.Render(g,b.Size,values,pressed?.8f:0,pressed?-.4f:0,0,0,10,14,pressed?"X":null);
                b.Save(Path.Combine(folder,pressed?"joystick-activo.png":"joystick.png"));
            }
            result.Add("PASS: idle and active drawings rendered without opening windows.");
            using var stream=typeof(VisualCheck).Assembly.GetManifestResourceStream("GameSirSimple.app.ico")??throw new Exception("Missing icon");using var icon=new Icon(stream);if(icon.Width<16)throw new Exception("Invalid icon");
            result.Add("PASS: application icon embedded and readable.");
            using(var window=new MainWindow(true)){
                var controls=Descendants(window).ToArray();
                if(controls.Any(c=>c.Text.Contains("x360ce",StringComparison.OrdinalIgnoreCase) || c.Text.Contains("HidHide") || c.Text.Contains("Steam Input")))throw new Exception("External-app instructions still present in UI");
                var tabs=controls.OfType<TabControl>().Single();
                foreach(var control in controls){_ = control.Handle;control.PerformLayout();}
                var languages=controls.OfType<LanguagePicker>().Single();
                foreach(int language in new[]{0,1,2,0}){
                    languages.SelectedIndex=language;
                    string code=new[]{"es","en","zh-CN"}[language];
                    if(languages.Text!=new[]{"Español ▾","English ▾","简体中文 ▾"}[language])throw new Exception("Language selector text is missing");
                    if(I18n.Language!=code || tabs.TabPages[0].Text!=new[]{"Joystick interactivo","Interactive controller","交互式手柄"}[language] || tabs.TabPages[2].Text!=new[]{"Botones y ejes","Buttons and axes","按键与坐标轴"}[language])throw new Exception("Live language switch failed: "+code);
                    var grid=controls.OfType<DataGridView>().Single();
                    if((string)grid.Rows[11].Cells[0].Value!=new[]{"Cruceta arriba","D-pad up","方向键上"}[language])throw new Exception("Mapping names not translated");
                    if(window.TrayLabels()[2]!=new[]{"Salir completamente","Exit completely","完全退出"}[language])throw new Exception("Tray menu not translated");
                    for(int i=0;i<tabs.TabPages.Count;i++){
                        tabs.SelectedIndex=i;window.PerformLayout();
                        foreach(var control in Descendants(tabs.TabPages[i]))control.PerformLayout();
                        using var frame=new Bitmap(window.Width,window.Height);window.DrawToBitmap(frame,new Rectangle(Point.Empty,frame.Size));frame.Save(Path.Combine(folder,$"app-{code}-tab-{i+1}.png"));
                        var colors=new HashSet<int>();for(int y=60;y<frame.Height-20;y+=7)for(int x=20;x<frame.Width-20;x+=7)colors.Add(frame.GetPixel(x,y).ToArgb());if(colors.Count<25)throw new Exception("Offscreen screen is blank: "+i);
                    }
                }
            }
            result.Add("PASS: live ES/EN/ZH language switching and back, mapping labels, tray translations and saved language schema.");
            result.Add("PASS: all three screens rendered in all three languages offscreen; no external-app banner or saved-setting changes.");
            if(!MainWindow.KeepInTray(CloseReason.UserClosing,false) || MainWindow.KeepInTray(CloseReason.UserClosing,true) || MainWindow.KeepInTray(CloseReason.WindowsShutDown,false) || MainWindow.KeepInTray(CloseReason.ApplicationExitCall,false))throw new Exception("Tray closing policy regression");
            result.Add("PASS: X keeps running; explicit exit and Windows shutdown permit complete closing.");
        }catch(Exception ex){Environment.ExitCode=1;result.Add("FAIL: "+ex);}
        File.WriteAllLines(Path.Combine(folder,"visual-check.txt"),result);
    }
}
