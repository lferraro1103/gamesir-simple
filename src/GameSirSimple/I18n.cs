using System.Globalization;
namespace GameSirSimple;

static class I18n
{
    public static string Language="es";
    public static bool Supported(string? language)=>language is "es" or "en" or "zh-CN";
    // Spanish keys are also the default UI text; stored control IDs never change.
    static readonly Dictionary<string,string[]> Texts=new()
    {
        ["Idioma"]=new[]{"Language","语言"},
        ["Abrir GameSir Simple"]=new[]{"Open GameSir Simple","打开 GameSir Simple"},
        ["Salir completamente"]=new[]{"Exit completely","完全退出"},
        ["GameSir Simple sigue funcionando en el área de notificaciones. Hacé doble clic en el icono para abrirlo."]=new[]{"GameSir Simple is still running in the notification area. Double-click its icon to open it.","GameSir Simple 仍在通知区域运行。双击图标可重新打开。"},
        ["Activar control Xbox"]=new[]{"Enable Xbox controller","启用 Xbox 手柄"},
        ["Desactivar Xbox"]=new[]{"Disable Xbox","停用 Xbox"},
        ["Guardar ajustes"]=new[]{"Save settings","保存设置"},
        ["Joystick interactivo"]=new[]{"Interactive controller","交互式手柄"},
        ["Palancas"]=new[]{"Sticks","摇杆"},
        ["Botones y ejes"]=new[]{"Buttons and axes","按键与坐标轴"},
        ["○ Conectá tu GameSir X5 Lite por USB"]=new[]{"○ Connect your GameSir X5 Lite via USB","○ 请通过 USB 连接 GameSir X5 Lite"},
        ["● Xbox activo · listo para jugar"]=new[]{"● Xbox enabled · ready to play","● Xbox 已启用 · 可以开始游戏"},
        ["● GameSir X5 Lite conectado · podés probarlo y configurar"]=new[]{"● GameSir X5 Lite connected · test and configure your inputs","● GameSir X5 Lite 已连接 · 可以测试并设置输入"},
        ["Hacé clic en el joystick para asignar un botón o una dirección.\nLos controles se iluminan al presionarlos. Clic derecho: restablecer."]=new[]{"Click the controller to assign a button or direction.\nInputs light up when pressed. Right-click to restore a mapping.","点击手柄示意图可映射按键或方向。\n操作时对应控件会亮起。右键点击可恢复默认映射。"},
        ["Zona muerta izquierda (%)"]=new[]{"Left stick dead zone (%)","左摇杆死区 (%)"},
        ["Zona muerta derecha (%)"]=new[]{"Right stick dead zone (%)","右摇杆死区 (%)"},
        ["Recorrido para llegar al máximo (%)"]=new[]{"Travel to reach maximum output (%)","达到最大输出所需行程 (%)"},
        ["Curva: 100 normal · >100 más suave al centro"]=new[]{"Response curve: 100 linear · >100 softer near center","响应曲线：100 为线性 · >100 时中心更柔和"},
        ["Invertir X izquierda"]=new[]{"Invert left X axis","反转左摇杆 X 轴"},
        ["Invertir Y izquierda"]=new[]{"Invert left Y axis","反转左摇杆 Y 轴"},
        ["Invertir X derecha"]=new[]{"Invert right X axis","反转右摇杆 X 轴"},
        ["Invertir Y derecha"]=new[]{"Invert right Y axis","反转右摇杆 Y 轴"},
        ["Calibrar centro (soltá las dos palancas)"]=new[]{"Calibrate center (release both sticks)","校准中心（请松开两个摇杆）"},
        ["Restablecer centro"]=new[]{"Reset center calibration","重置中心校准"},
        ["Zona muerta radial: elimina movimientos cerca del centro.\nBajá el recorrido máximo si la palanca no llega al borde.\nLos gatillos del X5 Lite son digitales: LT y RT funcionan como botones.\nLa calibración de centro se aplica a los ejes automáticos. Al reasignar\nun eje se toma su posición de reposo durante la grabación."]=new[]{"Radial dead zones remove movement near the center.\nReduce maximum travel if a stick cannot reach full output.\nX5 Lite triggers are digital: LT and RT act as buttons.\nCenter calibration applies to automatic axes. A custom axis binding\ncaptures its resting position while recording.","径向死区可消除中心附近的微小移动。\n如果摇杆无法达到最大输出，请降低所需行程。\nX5 Lite 的扳机为数字式：LT 和 RT 相当于按键。\n中心校准适用于自动映射的坐标轴。\n自定义坐标轴映射会在录入时保存静止位置。"},
        ["Control Xbox"]=new[]{"Xbox control","Xbox 控件"},
        ["Asignación"]=new[]{"Mapping","映射"},
        ["Asignar"]=new[]{"Assign","映射"},
        ["Automático"]=new[]{"Default","默认"},
        ["Mapa GameSir X5 Lite"]=new[]{"GameSir X5 Lite mapping","GameSir X5 Lite 默认映射"},
        ["Asignar → soltá todo → presioná el botón o mové la palanca\nen la dirección indicada. Podés usar botones, ejes o cruceta.\nEsc cancela. La emulación se pausa mientras asignás."]=new[]{"Assign → release all controls → press a button or move a stick\nin the target direction. Buttons, axes and D-pad are supported.\nEsc cancels. Xbox emulation stops while recording.","点击映射 → 松开所有控件 → 按下按键或沿目标方向推动摇杆。\n支持按键、坐标轴与方向键。\nEsc 取消。录入期间会停止 Xbox 模拟。"},
        ["Activá el control Xbox para jugar. La X lo deja en el área de notificaciones.\nPara terminar: clic derecho en el icono → Salir completamente."]=new[]{"Enable the Xbox controller to play. The X keeps it running in the notification area.\nTo quit: right-click its icon → Exit completely.","启用 Xbox 手柄后即可游戏。点击 X 会保留在通知区域运行。\n如需退出：右键点击图标 → 完全退出。"},
        ["Ajustes guardados."]=new[]{"Settings saved.","设置已保存。"},
        ["No se pudieron guardar los ajustes."]=new[]{"Could not save settings.","无法保存设置。"},
        ["Conectá el GameSir para calibrar."]=new[]{"Connect the GameSir to calibrate.","请连接 GameSir 后再校准。"},
        ["Soltá las dos palancas y volvé a calibrar."]=new[]{"Release both sticks and try calibrating again.","请松开两个摇杆后重新校准。"},
        ["Centro calibrado para el mapa automático."]=new[]{"Center calibrated for automatic mappings.","自动映射的中心已校准。"},
        ["No se pudo crear el control Xbox. Verificá que el controlador ViGEmBus esté instalado."]=new[]{"Could not create the Xbox controller. Check that the ViGEmBus driver is installed.","无法创建 Xbox 手柄。请检查是否已安装 ViGEmBus 驱动。"},
        ["Conectá el GameSir primero."]=new[]{"Connect the GameSir first.","请先连接 GameSir。"},
        ["Soltá todo… después activá: {0}. Esc cancela."]=new[]{"Release all controls… then activate: {0}. Esc cancels.","松开所有控件… 然后操作：{0}。Esc 取消。"},
        ["Tiempo agotado. Tocá Asignar para repetir."]=new[]{"Timed out. Click Assign to try again.","操作超时。请点击映射重试。"},
        ["Ahora presioná o mové: {0}. Esc cancela."]=new[]{"Now press or move: {0}. Esc cancels.","现在按下或推动：{0}。Esc 取消。"},
        ["{0} → {1}. Volvé a activar Xbox cuando termines."]=new[]{"{0} → {1}. Enable Xbox again when finished.","{0} → {1}。完成后请重新启用 Xbox。"},
        ["Botones: {0}\nIzq. X {1:F2}  Y {2:F2}     Der. X {3:F2}  Y {4:F2}"]=new[]{"Buttons: {0}\nLeft X {1:F2}  Y {2:F2}     Right X {3:F2}  Y {4:F2}","按键：{0}\n左摇杆 X {1:F2}  Y {2:F2}     右摇杆 X {3:F2}  Y {4:F2}"},
        ["Asignando: {0} · {1}\nEsc cancela. Al terminar, volvé a activar Xbox."]=new[]{"Assigning: {0} · {1}\nEsc cancels. Enable Xbox again when finished.","正在映射：{0} · {1}\nEsc 取消。完成后请重新启用 Xbox。"},
        ["Soltá todos los controles…"]=new[]{"Release all controls…","请松开所有控件…"},
        ["Ahora presioná el botón o mové la palanca."]=new[]{"Now press a button or move the stick.","现在按下按键或推动摇杆。"},
        ["Se detuvo la emulación. Volvé a activar el control Xbox."]=new[]{"Emulation stopped. Enable the Xbox controller again.","模拟已停止，请重新启用 Xbox 手柄。"},
        ["Mapa automático"]=new[]{"Default mapping","默认映射"},
        ["Clic: asignar · Clic derecho: restablecer"]=new[]{"Click: assign · Right-click: restore","单击：映射 · 右键：恢复"},
        ["Restablecer {0} al mapa automático"]=new[]{"Restore {0} to its default mapping","恢复 {0} 的默认映射"},
        ["IZQUIERDA · ZONA MUERTA {0}%"]=new[]{"LEFT · DEAD ZONE {0}%","左摇杆 · 死区 {0}%"},
        ["DERECHA · ZONA MUERTA {0}%"]=new[]{"RIGHT · DEAD ZONE {0}%","右摇杆 · 死区 {0}%"},
        ["Botón {0}"]=new[]{"Button {0}","按键 {0}"},
        ["Cruceta {0} · {1}"]=new[]{"D-pad {0} · {1}","方向键 {0} · {1}"},
        ["Eje {0} {1}"]=new[]{"Axis {0} {1}","坐标轴 {0} {1}"},
        ["Cruceta arriba"]=new[]{"D-pad up","方向键上"},["Cruceta abajo"]=new[]{"D-pad down","方向键下"},
        ["Cruceta izquierda"]=new[]{"D-pad left","方向键左"},["Cruceta derecha"]=new[]{"D-pad right","方向键右"},
        ["Izq. ←"]=new[]{"Left stick ←","左摇杆 ←"},["Izq. →"]=new[]{"Left stick →","左摇杆 →"},
        ["Izq. ↑"]=new[]{"Left stick ↑","左摇杆 ↑"},["Izq. ↓"]=new[]{"Left stick ↓","左摇杆 ↓"},
        ["Der. ←"]=new[]{"Right stick ←","右摇杆 ←"},["Der. →"]=new[]{"Right stick →","右摇杆 →"},
        ["Der. ↑"]=new[]{"Right stick ↑","右摇杆 ↑"},["Der. ↓"]=new[]{"Right stick ↓","右摇杆 ↓"},
        ["Guía"]=new[]{"Guide","主页"},["Back"]=new[]{"Back","返回"},["Start"]=new[]{"Start","开始"},
        ["Conectá el GameSir X5 Lite por USB primero."]=new[]{"Connect the GameSir X5 Lite via USB first.","请先通过 USB 连接 GameSir X5 Lite。"},
        ["GameSir Simple ya está abierto."]=new[]{"GameSir Simple is already running.","GameSir Simple 已在运行。"},
        ["No se pudo iniciar: {0}"]=new[]{"Could not start: {0}","无法启动：{0}"},
        ["No se pudo iniciar la lectura USB."]=new[]{"Could not initialize USB input.","无法初始化 USB 输入。"},
        ["No se pudo cargar el mapa integrado del GameSir."]=new[]{"Could not load the embedded GameSir mapping.","无法加载内置 GameSir 映射。"},
        ["No se encontró el mapa integrado del GameSir."]=new[]{"The embedded GameSir mapping was not found.","未找到内置 GameSir 映射。"},
        ["El mapa integrado del GameSir está incompleto."]=new[]{"The embedded GameSir mapping is incomplete.","内置 GameSir 映射不完整。"},
    };
    public static string T(string key,params object[] args){
        string text=key;
        if(Language!="es" && Texts.TryGetValue(key,out var pair))text=pair[Language=="zh-CN"?1:0];
        return args.Length==0?text:string.Format(CultureInfo.GetCultureInfo(Language=="es"?"es-AR":Language=="zh-CN"?"zh-CN":"en-US"),text,args);
    }
    public static string Name(string canonical)=>T(canonical);
}
