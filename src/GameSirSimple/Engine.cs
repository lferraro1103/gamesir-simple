using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
namespace GameSirSimple;

static class SDL
{
    const string D="SDL2.dll";
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern int SDL_Init(uint flags);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern void SDL_Quit();
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern int SDL_SetHint([MarshalAs(UnmanagedType.LPUTF8Str)]string name,[MarshalAs(UnmanagedType.LPUTF8Str)]string value);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern int SDL_NumJoysticks();
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern IntPtr SDL_JoystickNameForIndex(int index);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern IntPtr SDL_GetError();
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern ushort SDL_JoystickGetDeviceVendor(int index);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern ushort SDL_JoystickGetDeviceProduct(int index);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern IntPtr SDL_GameControllerOpen(int index);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern void SDL_GameControllerClose(IntPtr controller);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern IntPtr SDL_GameControllerGetJoystick(IntPtr controller);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern int SDL_GameControllerGetAttached(IntPtr controller);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern short SDL_GameControllerGetAxis(IntPtr controller,int axis);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern byte SDL_GameControllerGetButton(IntPtr controller,int button);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern void SDL_PumpEvents();
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern void SDL_GameControllerUpdate();
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern int SDL_JoystickNumAxes(IntPtr joystick);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern int SDL_JoystickNumButtons(IntPtr joystick);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern int SDL_JoystickNumHats(IntPtr joystick);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern short SDL_JoystickGetAxis(IntPtr joystick,int axis);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern byte SDL_JoystickGetButton(IntPtr joystick,int button);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern byte SDL_JoystickGetHat(IntPtr joystick,int hat);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern IntPtr SDL_RWFromConstMem(IntPtr memory,int size);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern int SDL_GameControllerAddMappingsFromRW(IntPtr rw,int free);
    public static void Initialize() {
        SDL_SetHint("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS","1"); SDL_SetHint("SDL_JOYSTICK_HIDAPI","0");
        if(SDL_Init(0x200|0x2000|0x4000)!=0)throw new Exception(I18n.T("No se pudo iniciar la lectura USB."));
        try {
            byte[] data=EmbeddedMapping();
            IntPtr memory=Marshal.AllocHGlobal(data.Length);
            try {
                Marshal.Copy(data,0,memory,data.Length);var rw=SDL_RWFromConstMem(memory,data.Length);
                if(rw==IntPtr.Zero || SDL_GameControllerAddMappingsFromRW(rw,1)<1)throw new Exception(I18n.T("No se pudo cargar el mapa integrado del GameSir."));
            }finally{Marshal.FreeHGlobal(memory);}
        }catch{SDL_Quit();throw;}
    }
    public static byte[] EmbeddedMapping(){
        using var stream=typeof(SDL).Assembly.GetManifestResourceStream("GameSirSimple.gamecontrollerdb.txt")??throw new Exception(I18n.T("No se encontró el mapa integrado del GameSir."));
        using var reader=new StreamReader(stream);var lines=reader.ReadToEnd().Split('\n').Where(l=>l.Contains(",GameSir X5 Lite,") && l.Contains("platform:Windows,"));
        string mapping=string.Join("\n",lines).Trim();
        if(mapping.Length==0)throw new Exception(I18n.T("El mapa integrado del GameSir está incompleto."));
        return Encoding.UTF8.GetBytes(mapping+"\n");
    }
}

public record Binding(string Kind,int Index,int Sign,float Center) {
    public override string ToString()=>Kind=="button"?I18n.T("Botón {0}",Index+1):Kind=="hat"?I18n.T("Cruceta {0} · {1}",Index+1,Sign):I18n.T("Eje {0} {1}",Index+1,Sign>0?"+":"−");
    public float AxisValue(float raw)=>float.IsFinite(raw)&&float.IsFinite(Center)?Math.Clamp((raw-Center)*Sign/Math.Max(.05f,1-Center*Sign),0,1):0;
}
public class Settings {
    public string Language{get;set;}="es";
    public int LeftDead{get;set;}=10;
    public int RightDead{get;set;}=10;
    public int Outer{get;set;}=100;
    public int Response{get;set;}=100;
    public bool[] Invert{get;set;}=new bool[4];
    public float[] Centers{get;set;}=new float[4];
    public Dictionary<string,Binding> Bindings{get;set;}=new();
    public static string FilePath=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"GameSirSimple","settings.json");
    public static Settings Load() {
        try {
            var s=JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath))??new();
            s.Normalize();return s;
        }catch{return new();}
    }
    public void Normalize(){
        if(!I18n.Supported(Language))Language="es";
        LeftDead=Math.Clamp(LeftDead,0,40);RightDead=Math.Clamp(RightDead,0,40);Outer=Math.Clamp(Outer,60,100);Response=Math.Clamp(Response,50,200);
        if(Invert==null || Invert.Length!=4)Invert=new bool[4];
        if(Centers==null || Centers.Length!=4)Centers=new float[4];
        for(int i=0;i<4;i++)if(!float.IsFinite(Centers[i]) || Math.Abs(Centers[i])>.35f)Centers[i]=0;
        Bindings??=new();
        foreach(var entry in Bindings.ToArray()){
            var b=entry.Value;
            if(!Engine.Names.Contains(entry.Key) || b==null || b.Index<0 || b.Index>255 || !float.IsFinite(b.Center) || Math.Abs(b.Center)>1 ||
                !(b.Kind=="button" || b.Kind=="hat" && new[]{1,2,4,8}.Contains(b.Sign) || b.Kind=="axis" && b.Sign is -1 or 1))Bindings.Remove(entry.Key);
        }
    }
    public void Save(){
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        string temp=FilePath+".tmp";
        File.WriteAllText(temp,JsonSerializer.Serialize(this,new JsonSerializerOptions{WriteIndented=true}));File.Move(temp,FilePath,true);
    }
}

class Engine:IDisposable {
    public static readonly string[] Names={"A","B","X","Y","Back","Guía","Start","L3","R3","LB","RB","Cruceta arriba","Cruceta abajo","Cruceta izquierda","Cruceta derecha","LT","RT","Izq. ←","Izq. →","Izq. ↑","Izq. ↓","Der. ←","Der. →","Der. ↑","Der. ↓"};
    static readonly Xbox360Button[] Buttons={Xbox360Button.A,Xbox360Button.B,Xbox360Button.X,Xbox360Button.Y,Xbox360Button.Back,Xbox360Button.Guide,Xbox360Button.Start,Xbox360Button.LeftThumb,Xbox360Button.RightThumb,Xbox360Button.LeftShoulder,Xbox360Button.RightShoulder,Xbox360Button.Up,Xbox360Button.Down,Xbox360Button.Left,Xbox360Button.Right};
    public Settings Config;
    public IntPtr Pad,Joy;
    ViGEmClient? bus; IXbox360Controller? output;
    readonly bool initialized;
    public bool Active=>output!=null;
    public bool Connected=>Pad!=IntPtr.Zero;
    public float[] Values=new float[25];
    public float LX,LY,RX,RY;
    public Engine(Settings config,bool initialize=true){Config=config;if(initialize){SDL.Initialize();initialized=true;}}
    public void Discover() {
        SDL.SDL_PumpEvents();SDL.SDL_GameControllerUpdate();
        if(Connected && SDL.SDL_GameControllerGetAttached(Pad)==0)Detach();
        if(Connected)return;
        for(int i=0;i<SDL.SDL_NumJoysticks();i++) {
            if(SDL.SDL_JoystickGetDeviceVendor(i)!=0x3537 || SDL.SDL_JoystickGetDeviceProduct(i)!=0x1116)continue;
            Pad=SDL.SDL_GameControllerOpen(i);
            if(Connected){
                Joy=SDL.SDL_GameControllerGetJoystick(Pad);
                if(SDL.SDL_JoystickNumAxes(Joy)>=4 && SDL.SDL_JoystickNumButtons(Joy)>=10)break;
                SDL.SDL_GameControllerClose(Pad);Pad=Joy=IntPtr.Zero;
            }
        }
    }
    public float[] Axes()=>Enumerable.Range(0,SDL.SDL_JoystickNumAxes(Joy)).Select(i=>SDL.SDL_JoystickGetAxis(Joy,i)/32768f).ToArray();
    public byte[] RawButtons()=>Enumerable.Range(0,SDL.SDL_JoystickNumButtons(Joy)).Select(i=>SDL.SDL_JoystickGetButton(Joy,i)).ToArray();
    public byte[] Hats()=>Enumerable.Range(0,SDL.SDL_JoystickNumHats(Joy)).Select(i=>SDL.SDL_JoystickGetHat(Joy,i)).ToArray();
    public float ReadBinding(Binding b) {
        if(b.Index<0)return 0;
        if(b.Kind=="button")return b.Index<SDL.SDL_JoystickNumButtons(Joy)?SDL.SDL_JoystickGetButton(Joy,b.Index):0;
        if(b.Kind=="hat")return b.Index<SDL.SDL_JoystickNumHats(Joy) && (SDL.SDL_JoystickGetHat(Joy,b.Index)&b.Sign)!=0?1:0;
        if(b.Kind!="axis" || b.Index>=SDL.SDL_JoystickNumAxes(Joy))return 0;
        float raw=SDL.SDL_JoystickGetAxis(Joy,b.Index)/32768f;
        return b.AxisValue(raw);
    }
    public void Read() {
        if(!Connected){ClearInput();return;}
        SDL.SDL_PumpEvents();SDL.SDL_GameControllerUpdate();
        if(SDL.SDL_GameControllerGetAttached(Pad)==0){Detach();return;}
        for(int i=0;i<15;i++)Values[i]=SDL.SDL_GameControllerGetButton(Pad,i);
        Values[15]=Math.Max(0,SDL.SDL_GameControllerGetAxis(Pad,4)/32767f);Values[16]=Math.Max(0,SDL.SDL_GameControllerGetAxis(Pad,5)/32767f);
        for(int a=0;a<4;a++){float v=SDL.SDL_GameControllerGetAxis(Pad,a)/32768f-Config.Centers[a];Values[17+a*2]=Math.Clamp(-v,0,1);Values[18+a*2]=Math.Clamp(v,0,1);}
        for(int i=0;i<Names.Length;i++)if(Config.Bindings.TryGetValue(Names[i],out var b))Values[i]=ReadBinding(b);
        float[] axes={Values[18]-Values[17],Values[20]-Values[19],Values[22]-Values[21],Values[24]-Values[23]};
        for(int i=0;i<4;i++)if(Config.Invert[i])axes[i]=-axes[i];
        (LX,LY)=Shape(axes[0],axes[1],Config.LeftDead/100f,Config.Outer/100f,Config.Response/100f);
        (RX,RY)=Shape(axes[2],axes[3],Config.RightDead/100f,Config.Outer/100f,Config.Response/100f);
    }
    public static (float x,float y) Shape(float x,float y,float dead,float outer,float response){float r=MathF.Sqrt(x*x+y*y);if(r<=dead || r==0)return(0,0);float strength=MathF.Pow(Math.Clamp((r-dead)/(outer-dead),0,1),response);return(x/r*strength,y/r*strength);}
    public void Start(){if(!Connected)throw new Exception(I18n.T("Conectá el GameSir X5 Lite por USB primero."));Stop();try{bus=new ViGEmClient();output=bus.CreateXbox360Controller();output.AutoSubmitReport=false;output.Connect();}catch{Stop();throw;}}
    public void Submit() {
        if(output==null)return;
        for(int i=0;i<15;i++)output.SetButtonState(Buttons[i],Values[i]>.5f);
        output.SetSliderValue(Xbox360Slider.LeftTrigger,(byte)(Values[15]*255));output.SetSliderValue(Xbox360Slider.RightTrigger,(byte)(Values[16]*255));
        output.SetAxisValue(Xbox360Axis.LeftThumbX,(short)(LX*32767));output.SetAxisValue(Xbox360Axis.LeftThumbY,(short)(-LY*32767));output.SetAxisValue(Xbox360Axis.RightThumbX,(short)(RX*32767));output.SetAxisValue(Xbox360Axis.RightThumbY,(short)(-RY*32767));output.SubmitReport();
    }
    public void Stop(){if(output!=null){try{output.Disconnect();}catch{}(output as IDisposable)?.Dispose();output=null;}bus?.Dispose();bus=null;}
    public void ClearInput(){Array.Clear(Values);LX=LY=RX=RY=0;}
    void Detach(){try{Stop();}finally{SDL.SDL_GameControllerClose(Pad);Pad=Joy=IntPtr.Zero;ClearInput();}}
    public void Dispose(){Stop();if(Connected)SDL.SDL_GameControllerClose(Pad);Pad=Joy=IntPtr.Zero;ClearInput();if(initialized)SDL.SDL_Quit();}
}

