using System.Runtime.InteropServices;
namespace GameSirSimple;
static class Diagnostics {
    [StructLayout(LayoutKind.Sequential)]struct Pad{public ushort Buttons;public byte LT,RT;public short LX,LY,RX,RY;}
    [StructLayout(LayoutKind.Sequential)]struct State{public uint Packet;public Pad Gamepad;}
    [DllImport("xinput1_4.dll")]static extern uint XInputGetState(uint index,out State state);
    public static void Run() {
        var lines=new List<string>();try {
            if(Engine.Shape(.03f,.02f,.1f,1,1)!=(0f,0f))throw new Exception("Dead zone failed");
            if(Math.Abs(Engine.Shape(1,0,.1f,1,1).x-1)>.001)throw new Exception("Outer range failed");
            var diagonal=Engine.Shape(1,1,.1f,1,1);if(diagonal.x*diagonal.x+diagonal.y*diagonal.y>1.001)throw new Exception("Radial clamp failed");
            lines.Add("PASS: radial dead zone, full range, diagonal normalization");
            using var eng=new Engine(new Settings());
            for(int t=0;t<10 && !eng.Connected;t++){eng.Discover();Thread.Sleep(100);}
            for(int i=0;i<SDL.SDL_NumJoysticks();i++)lines.Add($"SDL device: {Marshal.PtrToStringUTF8(SDL.SDL_JoystickNameForIndex(i))} VID={SDL.SDL_JoystickGetDeviceVendor(i):X4} PID={SDL.SDL_JoystickGetDeviceProduct(i):X4}");
            lines.Add("SDL error: "+Marshal.PtrToStringUTF8(SDL.SDL_GetError()));
            lines.Add("GameSir X5 Lite connected: "+eng.Connected);
            if(!eng.Connected)throw new Exception("GameSir not found");eng.Read();lines.Add($"Raw axes: {eng.Axes().Length}; buttons: {eng.RawButtons().Length}; hats: {eng.Hats().Length}");
            eng.Start();eng.Values[0]=1;eng.Values[15]=1;eng.LX=.5f;eng.LY=-.5f;eng.Submit();Thread.Sleep(500);bool found=false;
            for(uint i=0;i<4;i++)if(XInputGetState(i,out var s)==0 && (s.Gamepad.Buttons&0x1000)!=0 && s.Gamepad.LT==255 && Math.Abs(s.Gamepad.LX-16383)<2 && Math.Abs(s.Gamepad.LY-16383)<2)found=true;
            if(!found)throw new Exception("Virtual Xbox report was not received through XInput");
            lines.Add("PASS: actual ViGEm virtual Xbox creation + A / LT / X,Y report verified through Windows XInput");eng.Stop();lines.Add("PASS: virtual controller disconnected");
        }catch(Exception ex){lines.Add("FAIL: "+ex);Environment.ExitCode=1;}
        File.WriteAllLines(Path.Combine(AppContext.BaseDirectory,"diagnostic.txt"),lines);
    }
}
