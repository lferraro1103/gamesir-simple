using System.Runtime.InteropServices;
using System.Text.Json;
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
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern IntPtr SDL_RWFromFile([MarshalAs(UnmanagedType.LPUTF8Str)]string path,string mode);
    [DllImport(D,CallingConvention=CallingConvention.Cdecl)] public static extern int SDL_GameControllerAddMappingsFromRW(IntPtr rw,int free);
    public static void Initialize() {
        SDL_SetHint("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS","1"); SDL_SetHint("SDL_JOYSTICK_HIDAPI","0");
        if(SDL_Init(0x200|0x2000|0x4000)!=0)throw new Exception("No se pudo iniciar la lectura USB.");
        var rw=SDL_RWFromFile(Path.Combine(AppContext.BaseDirectory,"gamecontrollerdb.txt"),"rb");
        if(rw==IntPtr.Zero || SDL_GameControllerAddMappingsFromRW(rw,1)<0)throw new Exception("Falta el mapa gamecontrollerdb.txt junto al programa.");
    }
}

public record Binding(string Kind,int Index,int Sign,float Center) {
    public override string ToString()=>Kind=="button"?$"Botón {Index+1}":Kind=="hat"?$"Cruceta {Index+1} · {Sign}":$"Eje {Index+1} {(Sign>0?"+":"−")}";
}
public class Settings {
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
            s.LeftDead=Math.Clamp(s.LeftDead,0,40);s.RightDead=Math.Clamp(s.RightDead,0,40);s.Outer=Math.Clamp(s.Outer,60,100);s.Response=Math.Clamp(s.Response,50,200);
            if(s.Invert==null || s.Invert.Length!=4)s.Invert=new bool[4];
            if(s.Centers==null || s.Centers.Length!=4)s.Centers=new float[4];
            s.Bindings??=new();return s;
        }catch{return new();}
    }
    public void Save(){Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);File.WriteAllText(FilePath,JsonSerializer.Serialize(this,new JsonSerializerOptions{WriteIndented=true}));}
}

class Engine:IDisposable {
    public static readonly string[] Names={"A","B","X","Y","Back","Guía","Start","L3","R3","LB","RB","Cruceta arriba","Cruceta abajo","Cruceta izquierda","Cruceta derecha","LT","RT","Izq. ←","Izq. →","Izq. ↑","Izq. ↓","Der. ←","Der. →","Der. ↑","Der. ↓"};
    static readonly Xbox360Button[] Buttons={Xbox360Button.A,Xbox360Button.B,Xbox360Button.X,Xbox360Button.Y,Xbox360Button.Back,Xbox360Button.Guide,Xbox360Button.Start,Xbox360Button.LeftThumb,Xbox360Button.RightThumb,Xbox360Button.LeftShoulder,Xbox360Button.RightShoulder,Xbox360Button.Up,Xbox360Button.Down,Xbox360Button.Left,Xbox360Button.Right};
    public Settings Config;
    public IntPtr Pad,Joy;
    ViGEmClient? bus; IXbox360Controller? output;
    public bool Active=>output!=null;
    public bool Connected=>Pad!=IntPtr.Zero;
    public float[] Values=new float[25];
    public float LX,LY,RX,RY;
    public Engine(Settings config){Config=config;SDL.Initialize();}
    public void Discover() {
        SDL.SDL_PumpEvents();SDL.SDL_GameControllerUpdate();
        if(Connected && SDL.SDL_GameControllerGetAttached(Pad)==0){Stop();SDL.SDL_GameControllerClose(Pad);Pad=Joy=IntPtr.Zero;}
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
        return Math.Clamp((raw-b.Center)*b.Sign/Math.Max(.05f,1-b.Center*b.Sign),0,1);
    }
    public void Read() {
        if(!Connected){Array.Clear(Values);LX=LY=RX=RY=0;return;}
        SDL.SDL_PumpEvents();SDL.SDL_GameControllerUpdate();
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
    public void Start(){if(!Connected)throw new Exception("Conectá el GameSir X5 Lite por USB primero.");Stop();try{bus=new ViGEmClient();output=bus.CreateXbox360Controller();output.AutoSubmitReport=false;output.Connect();}catch{Stop();throw;}}
    public void Submit() {
        if(output==null)return;
        for(int i=0;i<15;i++)output.SetButtonState(Buttons[i],Values[i]>.5f);
        output.SetSliderValue(Xbox360Slider.LeftTrigger,(byte)(Values[15]*255));output.SetSliderValue(Xbox360Slider.RightTrigger,(byte)(Values[16]*255));
        output.SetAxisValue(Xbox360Axis.LeftThumbX,(short)(LX*32767));output.SetAxisValue(Xbox360Axis.LeftThumbY,(short)(-LY*32767));output.SetAxisValue(Xbox360Axis.RightThumbX,(short)(RX*32767));output.SetAxisValue(Xbox360Axis.RightThumbY,(short)(-RY*32767));output.SubmitReport();
    }
    public void Stop(){if(output!=null){try{output.Disconnect();}catch{}(output as IDisposable)?.Dispose();output=null;}bus?.Dispose();bus=null;}
    public void Dispose(){Stop();if(Connected)SDL.SDL_GameControllerClose(Pad);SDL.SDL_Quit();}
}

