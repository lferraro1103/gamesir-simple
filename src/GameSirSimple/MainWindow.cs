namespace GameSirSimple;

class MainWindow:Form {
    readonly Settings config;
    readonly Dictionary<Control,string> staticText=new();
    readonly LanguagePicker languagePicker=new(){Width=150,Height=32};
    readonly NotifyIcon tray=new(){Text="GameSir Simple"};
    readonly ContextMenuStrip trayMenu=new();
    readonly ToolStripMenuItem trayOpen=new(),trayToggle=new(),trayExit=new();
    bool exitRequested,trayNoticeShown,resourcesDisposed;
    readonly Engine engine;
    readonly Label status=new(){AutoSize=true},notice=new(){Dock=DockStyle.Bottom,Height=26,Padding=new Padding(20,0,0,0)};
    readonly Button toggle=new(){Text="Activar control Xbox",AutoSize=true,Padding=new Padding(16,5,16,5),BackColor=Color.FromArgb(30,100,220),ForeColor=Color.White,FlatStyle=FlatStyle.Flat,FlatAppearance={BorderSize=0}};
    readonly System.Windows.Forms.Timer timer=new(){Interval=8};
    readonly DataGridView grid=new();
    readonly ControllerView preview=new(){Dock=DockStyle.Fill};
    readonly Label mappingPrompt=new(){Dock=DockStyle.Top,Height=58,Padding=new Padding(12,8,12,4),Text="Hacé clic en el joystick para asignar un botón o una dirección.\nLos controles se iluminan al presionarlos. Clic derecho: restablecer.",BackColor=Color.FromArgb(235,242,251)};
    readonly Label live=new(){Dock=DockStyle.Bottom,Height=74,Padding=new Padding(12),AutoEllipsis=true};
    string? recording;
    float[] baseline=Array.Empty<float>();byte[] baselineButtons=Array.Empty<byte>(),baselineHats=Array.Empty<byte>();
    DateTime recordAt,recordUntil;int ticks;bool ready;
    public MainWindow(bool previewOnly=false,Settings? settings=null) {
        config=settings??(previewOnly?new Settings():Settings.Load());I18n.Language=config.Language;
        Text="GameSir Simple · X5 Lite";Font=new Font("Segoe UI",10);ClientSize=new Size(940,820);MinimumSize=new Size(860,760);StartPosition=FormStartPosition.CenterScreen;BackColor=Color.FromArgb(245,247,251);
        using(var resource=typeof(MainWindow).Assembly.GetManifestResourceStream("GameSirSimple.app.ico"))if(resource!=null)Icon=new Icon(resource);
        engine=new Engine(config,!previewOnly);
        status.Text="○ Conectá tu GameSir X5 Lite por USB";toggle.Enabled=false;
        var root=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(20),ColumnCount=1,RowCount=5};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,46));root.RowStyles.Add(new RowStyle(SizeType.Absolute,34));root.RowStyles.Add(new RowStyle(SizeType.Absolute,54));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,56));
        root.Controls.Add(new Label{Text="GameSir Simple",Font=new Font("Segoe UI",21,FontStyle.Bold),AutoSize=true},0,0);root.Controls.Add(status,0,1);
        var actions=new FlowLayoutPanel{Dock=DockStyle.Fill};toggle.Click+=(_,_)=>Toggle();actions.Controls.Add(toggle);
        var save=new Button{Text="Guardar ajustes",AutoSize=true,Padding=new Padding(6)};save.Click+=(_,_)=>Save();actions.Controls.Add(save);
        actions.Controls.Add(new Label{Text="Idioma",AutoSize=true,Margin=new Padding(18,12,4,0)});
        languagePicker.SelectedIndex=config.Language=="en"?1:config.Language=="zh-CN"?2:0;languagePicker.Margin=new Padding(3,7,3,3);actions.Controls.Add(languagePicker);root.Controls.Add(actions,0,2);
        var tabs=new TabControl{Dock=DockStyle.Fill};root.Controls.Add(tabs,0,3);
        var test=new TabPage("Joystick interactivo"){Padding=new Padding(8)};test.Controls.Add(preview);test.Controls.Add(live);test.Controls.Add(mappingPrompt);tabs.TabPages.Add(test);
        preview.MappingText=n=>config.Bindings.TryGetValue(n,out var b)?b.ToString():I18n.T("Mapa GameSir X5 Lite");
        preview.AssignRequested+=BeginRecord;
        preview.ResetRequested+=n=>{CancelRecord();config.Bindings.Remove(n);UpdateGrid();AutoSave();};
        var sticks=new TabPage("Palancas"){Padding=new Padding(16),AutoScroll=true};tabs.TabPages.Add(sticks);
        var options=new TableLayoutPanel{Dock=DockStyle.Top,AutoSize=true,ColumnCount=2};options.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));options.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));sticks.Controls.Add(options);
        AddNumber(options,"Zona muerta izquierda (%)",config.LeftDead,0,40,v=>config.LeftDead=v);
        AddNumber(options,"Zona muerta derecha (%)",config.RightDead,0,40,v=>config.RightDead=v);
        AddNumber(options,"Recorrido para llegar al máximo (%)",config.Outer,60,100,v=>config.Outer=v);
        AddNumber(options,"Curva: 100 normal · >100 más suave al centro",config.Response,50,200,v=>config.Response=v);
        for(int i=0;i<4;i++) {
            int a=i;var cb=new CheckBox{Text="Invertir "+new[]{"X izquierda","Y izquierda","X derecha","Y derecha"}[i],AutoSize=true,Checked=config.Invert[i],Margin=new Padding(3,8,0,0)};
            cb.CheckedChanged+=(_,_)=>{config.Invert[a]=cb.Checked;AutoSave();};AddWide(options,cb);
        }
        var calibrate=new Button{Text="Calibrar centro (soltá las dos palancas)",AutoSize=true,Margin=new Padding(3,16,0,0)};
        calibrate.Click+=(_,_)=>{
            if(!engine.Connected){notice.Text=I18n.T("Conectá el GameSir para calibrar.");return;}
            var centers=Enumerable.Range(0,4).Select(i=>SDL.SDL_GameControllerGetAxis(engine.Pad,i)/32768f).ToArray();
            if(centers.Any(v=>Math.Abs(v)>.35f)){notice.Text=I18n.T("Soltá las dos palancas y volvé a calibrar.");return;}
            config.Centers=centers;AutoSave();notice.Text=I18n.T("Centro calibrado para el mapa automático.");
        };AddWide(options,calibrate);
        var reset=new Button{Text="Restablecer centro",AutoSize=true,Margin=new Padding(3,8,0,0)};reset.Click+=(_,_)=>{Array.Clear(config.Centers);AutoSave();};AddWide(options,reset);
        AddWide(options,new Label{Text="Zona muerta radial: elimina movimientos cerca del centro.\nBajá el recorrido máximo si la palanca no llega al borde.\nLos gatillos del X5 Lite son digitales: LT y RT funcionan como botones.\nLa calibración de centro se aplica a los ejes automáticos. Al reasignar\nun eje se toma su posición de reposo durante la grabación.",AutoSize=true,Margin=new Padding(3,16,0,0)});
        var mapping=new TabPage("Botones y ejes"){Padding=new Padding(8)};tabs.TabPages.Add(mapping);
        grid.Dock=DockStyle.Fill;grid.AllowUserToAddRows=false;grid.AllowUserToDeleteRows=false;grid.RowHeadersVisible=false;grid.ReadOnly=true;grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;grid.MultiSelect=false;grid.BackgroundColor=Color.White;grid.BorderStyle=BorderStyle.None;
        grid.EnableHeadersVisualStyles=false;grid.ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.DisableResizing;grid.ColumnHeadersHeight=34;grid.RowTemplate.Height=28;
        grid.ColumnHeadersDefaultCellStyle=new DataGridViewCellStyle{BackColor=Color.FromArgb(235,242,251),ForeColor=Color.FromArgb(31,46,67),SelectionBackColor=Color.FromArgb(235,242,251),SelectionForeColor=Color.FromArgb(31,46,67),Font=new Font("Segoe UI",10,FontStyle.Bold),Padding=new Padding(4,0,0,0)};
        grid.DefaultCellStyle.SelectionBackColor=Color.White;grid.DefaultCellStyle.SelectionForeColor=Color.FromArgb(31,46,67);grid.DefaultCellStyle.Padding=new Padding(4,0,0,0);
        grid.DataBindingComplete+=(_,_)=>grid.ClearSelection();grid.VisibleChanged+=(_,_)=>{if(grid.Visible)grid.ClearSelection();};
        grid.Columns.Add("target","Control Xbox");grid.Columns.Add("binding","Asignación");
        grid.Columns.Add(new DataGridViewButtonColumn{HeaderText="",Text="Asignar",UseColumnTextForButtonValue=true,FillWeight=48});
        grid.Columns.Add(new DataGridViewButtonColumn{HeaderText="",Text="Automático",UseColumnTextForButtonValue=true,FillWeight=60});
        foreach(var n in Engine.Names)grid.Rows.Add(n,config.Bindings.TryGetValue(n,out var b)?b.ToString():"Mapa GameSir X5 Lite");
        foreach(DataGridViewColumn col in grid.Columns)col.SortMode=DataGridViewColumnSortMode.NotSortable;
        grid.CellContentClick+=(_,e)=>{if(e.RowIndex<0)return;string n=Engine.Names[e.RowIndex];if(e.ColumnIndex==2)BeginRecord(n);if(e.ColumnIndex==3){CancelRecord();config.Bindings.Remove(n);UpdateGrid();AutoSave();}};
        mapping.Controls.Add(grid);mapping.Controls.Add(new Label{Dock=DockStyle.Top,Height=68,Text="Asignar → soltá todo → presioná el botón o mové la palanca\nen la dirección indicada. Podés usar botones, ejes o cruceta.\nEsc cancela. La emulación se pausa mientras asignás."});
        root.Controls.Add(new Label{Text="Activá el control Xbox para jugar. La X lo deja en el área de notificaciones.\nPara terminar: clic derecho en el icono → Salir completamente.",Dock=DockStyle.Fill,Padding=new Padding(0,8,0,0)},0,4);
        Controls.Add(root);Controls.Add(notice);KeyPreview=true;KeyDown+=(_,e)=>{if(e.KeyCode==Keys.Escape)CancelRecord();};
        trayMenu.Items.AddRange(new ToolStripItem[]{trayOpen,trayToggle,new ToolStripSeparator(),trayExit});
        tray.Icon=Icon;tray.ContextMenuStrip=trayMenu;tray.Visible=!previewOnly;
        trayOpen.Click+=(_,_)=>RestoreWindow();tray.MouseClick+=(_,e)=>{if(e.Button==MouseButtons.Left)RestoreWindow();};tray.BalloonTipClicked+=(_,_)=>RestoreWindow();
        trayToggle.Click+=(_,_)=>{Toggle();RefreshView();};trayExit.Click+=(_,_)=>{exitRequested=true;Close();};
        trayMenu.Opening+=(_,_)=>RefreshTray();
        CaptureStaticText(this);ApplyLanguage();
        languagePicker.SelectedIndexChanged+=(_,_)=>{config.Language=languagePicker.SelectedItem.Code;ApplyLanguage();AutoSave();};
        FormClosing+=(_,e)=>{
            if(!previewOnly && KeepInTray(e.CloseReason,exitRequested)){
                e.Cancel=true;if(recording!=null)CancelRecord();Hide();
                if(!trayNoticeShown){trayNoticeShown=true;tray.ShowBalloonTip(3000,"GameSir Simple",I18n.T("GameSir Simple sigue funcionando en el área de notificaciones. Hacé doble clic en el icono para abrirlo."),ToolTipIcon.Info);}
                return;
            }
            AutoSave();
        };timer.Tick+=(_,_)=>TickInput();ready=!previewOnly;if(!previewOnly)timer.Start();
    }
    static void AddWide(TableLayoutPanel p,Control c){int row=p.RowCount++;p.Controls.Add(c,0,row);p.SetColumnSpan(c,2);}
    void AddNumber(TableLayoutPanel p,string label,int val,int min,int max,Action<int> change) {
        int row=p.RowCount++;p.RowStyles.Add(new RowStyle(SizeType.Absolute,44));p.Controls.Add(new Label{Text=label,AutoSize=true,Anchor=AnchorStyles.Left},0,row);
        // Minimum/Maximum must be set before Value: saved values above 100 would otherwise throw on startup.
        var num=new NumericUpDown{Minimum=min,Maximum=max,Value=Math.Clamp(val,min,max),Width=90,Anchor=AnchorStyles.Left};num.ValueChanged+=(_,_)=>{change((int)num.Value);AutoSave();};p.Controls.Add(num,1,row);
    }
    void Save(){try{config.Save();notice.Text=I18n.T("Ajustes guardados.");}catch{notice.Text=I18n.T("No se pudieron guardar los ajustes.");}}
    void AutoSave(){if(ready)Save();}
    void Toggle() {
        if(recording!=null)return;
        if(engine.Active)engine.Stop();else {
            try{engine.Start();}catch(Exception){notice.Text=I18n.T("No se pudo crear el control Xbox. Verificá que el controlador ViGEmBus esté instalado.");}
        }
    }
    void BeginRecord(string name){if(!engine.Connected){notice.Text=I18n.T("Conectá el GameSir primero.");return;}engine.Stop();recording=name;baseline=Array.Empty<float>();recordAt=DateTime.UtcNow.AddSeconds(1);recordUntil=DateTime.UtcNow.AddSeconds(12);notice.Text=I18n.T("Soltá todo… después activá: {0}. Esc cancela.",I18n.Name(name));}
    void CancelRecord(){recording=null;notice.Text="";}
    void UpdateGrid(){for(int i=0;i<Engine.Names.Length;i++){grid.Rows[i].Cells[0].Value=I18n.Name(Engine.Names[i]);grid.Rows[i].Cells[1].Value=config.Bindings.TryGetValue(Engine.Names[i],out var b)?b.ToString():I18n.T("Mapa GameSir X5 Lite");}}
    void RecordTick() {
        if(recording==null)return;
        if(DateTime.UtcNow>recordUntil){CancelRecord();notice.Text=I18n.T("Tiempo agotado. Tocá Asignar para repetir.");return;}
        var axes=engine.Axes();var buttons=engine.RawButtons();var hats=engine.Hats();
        if(DateTime.UtcNow<recordAt || baseline.Length!=axes.Length || baselineButtons.Length!=buttons.Length || baselineHats.Length!=hats.Length){baseline=axes;baselineButtons=buttons;baselineHats=hats;return;}
        notice.Text=I18n.T("Ahora presioná o mové: {0}. Esc cancela.",I18n.Name(recording));Binding? found=null;
        for(int i=0;i<buttons.Length;i++)if(buttons[i]!=0 && baselineButtons[i]==0){found=new("button",i,1,0);break;}
        if(found==null)for(int i=0;i<hats.Length;i++)if(hats[i]!=0 && hats[i]!=baselineHats[i]){int bit=new[]{1,2,4,8}.First(x=>(hats[i]&x)!=0);found=new("hat",i,bit,0);break;}
        if(found==null)for(int i=0;i<axes.Length;i++)if(Math.Abs(axes[i]-baseline[i])>.55f){found=new("axis",i,Math.Sign(axes[i]-baseline[i]),baseline[i]);break;}
        if(found!=null){string name=recording;config.Bindings[name]=found;recording=null;UpdateGrid();AutoSave();notice.Text=I18n.T("{0} → {1}. Volvé a activar Xbox cuando termines.",I18n.Name(name),found);}
    }
    void TickInput() {
        try {
            if(ticks++%60==0)engine.Discover();engine.Read();
            if(!engine.Connected && recording!=null)CancelRecord();if(engine.Connected)RecordTick();engine.Submit();if(ticks%6!=0 || !Visible)return;
            RefreshView();
        }catch{engine.Stop();notice.Text=I18n.T("Se detuvo la emulación. Volvé a activar el control Xbox.");}
    }
    void CaptureStaticText(Control parent){
        foreach(Control child in parent.Controls){
            // Only translatable captions: inputs such as NumericUpDown keep user values in Text.
            if(child is NumericUpDown)continue;
            if((child is Label or ButtonBase or TabPage) && child!=status && child!=notice && child!=mappingPrompt && child!=live && child!=toggle && child!=languagePicker && child.Text.Length>0)staticText[child]=child.Text;
            CaptureStaticText(child);
        }
    }
    void ApplyLanguage(){
        I18n.Language=config.Language;
        foreach(var pair in staticText)pair.Key.Text=I18n.T(pair.Value);
        grid.Columns[0].HeaderText=I18n.T("Control Xbox");grid.Columns[1].HeaderText=I18n.T("Asignación");
        ((DataGridViewButtonColumn)grid.Columns[2]).Text=I18n.T("Asignar");((DataGridViewButtonColumn)grid.Columns[3]).Text=I18n.T("Automático");
        preview.AccessibleName=I18n.T("Joystick interactivo");UpdateGrid();RefreshView();
    }
    void RefreshView(){
        status.Text=I18n.T(engine.Active?"● Xbox activo · listo para jugar":engine.Connected?"● GameSir X5 Lite conectado · podés probarlo y configurar":"○ Conectá tu GameSir X5 Lite por USB");
        status.ForeColor=engine.Connected?Color.FromArgb(20,125,65):Color.FromArgb(120,90,30);
        toggle.Text=I18n.T(engine.Active?"Desactivar Xbox":"Activar control Xbox");toggle.Enabled=engine.Connected && recording==null;
        // A flat button keeps its BackColor when disabled, so make the disabled state visible.
        toggle.BackColor=!toggle.Enabled?Color.FromArgb(196,205,218):engine.Active?Color.FromArgb(200,60,60):Color.FromArgb(30,100,220);toggle.ForeColor=toggle.Enabled?Color.White:Color.FromArgb(90,104,124);
        string buttons=string.Join("  ",Engine.Names.Take(17).Where((n,i)=>engine.Values[i]>.5f).Select(I18n.Name));if(buttons.Length==0)buttons="—";
        live.Text=I18n.T("Botones: {0}\nIzq. X {1:F2}  Y {2:F2}     Der. X {3:F2}  Y {4:F2}",buttons,engine.LX,engine.LY,engine.RX,engine.RY);
        preview.Values=engine.Values;preview.LX=engine.LX;preview.LY=engine.LY;preview.RX=engine.RX;preview.RY=engine.RY;preview.LeftDead=config.LeftDead;preview.RightDead=config.RightDead;preview.Recording=recording;
        mappingPrompt.Text=recording==null?I18n.T("Hacé clic en el joystick para asignar un botón o una dirección.\nLos controles se iluminan al presionarlos. Clic derecho: restablecer."):
            I18n.T("Asignando: {0} · {1}\nEsc cancela. Al terminar, volvé a activar Xbox.",I18n.Name(recording),I18n.T(DateTime.UtcNow<recordAt?"Soltá todos los controles…":"Ahora presioná el botón o mové la palanca."));
        mappingPrompt.BackColor=recording==null?Color.FromArgb(235,242,251):Color.FromArgb(255,239,208);preview.Invalidate();RefreshTray();
    }
    void RefreshTray(){trayOpen.Text=I18n.T("Abrir GameSir Simple");trayToggle.Text=I18n.T(engine.Active?"Desactivar Xbox":"Activar control Xbox");trayToggle.Enabled=engine.Connected && recording==null;trayExit.Text=I18n.T("Salir completamente");}
    void RestoreWindow(){Show();WindowState=FormWindowState.Normal;RefreshView();Activate();}
    internal static bool KeepInTray(CloseReason reason,bool quitting)=>reason==CloseReason.UserClosing && !quitting;
    internal string[] TrayLabels()=>trayMenu.Items.OfType<ToolStripMenuItem>().Select(item=>item.Text??"").ToArray();
    protected override void Dispose(bool disposing){if(disposing && !resourcesDisposed){resourcesDisposed=true;timer.Stop();timer.Dispose();tray.Visible=false;tray.Dispose();trayMenu.Dispose();engine.Dispose();}base.Dispose(disposing);}
}
