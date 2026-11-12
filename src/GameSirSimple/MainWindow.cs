namespace GameSirSimple;

class MainWindow:Form {
    readonly Settings config=Settings.Load();
    readonly Engine engine;
    readonly Label status=new(){AutoSize=true},notice=new(){Dock=DockStyle.Bottom,Height=26,Padding=new Padding(20,0,0,0)};
    readonly Button toggle=new(){Text="Activar control Xbox",AutoSize=true,Padding=new Padding(16,5,16,5),BackColor=Color.FromArgb(30,100,220),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};
    readonly System.Windows.Forms.Timer timer=new(){Interval=8};
    readonly DataGridView grid=new();
    readonly ControllerView preview=new(){Dock=DockStyle.Fill};
    readonly Label mappingPrompt=new(){Dock=DockStyle.Top,Height=58,Padding=new Padding(12,8,12,4),Text="Hacé clic en el joystick para asignar un botón o una dirección.\nLos controles se iluminan al presionarlos. Clic derecho: restablecer.",BackColor=Color.FromArgb(235,242,251)};
    readonly Label live=new(){Dock=DockStyle.Bottom,Height=74,Padding=new Padding(12),AutoEllipsis=true};
    string? recording;
    float[] baseline=Array.Empty<float>();byte[] baselineButtons=Array.Empty<byte>(),baselineHats=Array.Empty<byte>();
    DateTime recordAt,recordUntil;int ticks;bool ready;
    public MainWindow() {
        Text="GameSir Simple · X5 Lite";Font=new Font("Segoe UI",10);ClientSize=new Size(940,820);MinimumSize=new Size(860,760);StartPosition=FormStartPosition.CenterScreen;BackColor=Color.FromArgb(245,247,251);
        using(var resource=typeof(MainWindow).Assembly.GetManifestResourceStream("GameSirSimple.app.ico"))if(resource!=null)Icon=new Icon(resource);
        engine=new Engine(config);
        var root=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(20),ColumnCount=1,RowCount=5};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,46));root.RowStyles.Add(new RowStyle(SizeType.Absolute,34));root.RowStyles.Add(new RowStyle(SizeType.Absolute,54));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,56));
        root.Controls.Add(new Label{Text="GameSir Simple",Font=new Font("Segoe UI",21,FontStyle.Bold),AutoSize=true},0,0);root.Controls.Add(status,0,1);
        var actions=new FlowLayoutPanel{Dock=DockStyle.Fill};toggle.Click+=(_,_)=>Toggle();actions.Controls.Add(toggle);
        var save=new Button{Text="Guardar ajustes",AutoSize=true,Padding=new Padding(6)};save.Click+=(_,_)=>Save();actions.Controls.Add(save);root.Controls.Add(actions,0,2);
        var tabs=new TabControl{Dock=DockStyle.Fill};root.Controls.Add(tabs,0,3);
        var test=new TabPage("Joystick interactivo"){Padding=new Padding(8)};test.Controls.Add(preview);test.Controls.Add(live);test.Controls.Add(mappingPrompt);tabs.TabPages.Add(test);
        preview.MappingText=n=>config.Bindings.TryGetValue(n,out var b)?b.ToString():"Mapa GameSir X5 Lite";
        preview.AssignRequested+=BeginRecord;
        preview.ResetRequested+=n=>{CancelRecord();config.Bindings.Remove(n);UpdateGrid();AutoSave();};
        var sticks=new TabPage("Palancas"){Padding=new Padding(16),AutoScroll=true};tabs.TabPages.Add(sticks);
        var options=new TableLayoutPanel{Dock=DockStyle.Top,AutoSize=true,ColumnCount=2};options.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,78));options.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,22));sticks.Controls.Add(options);
        AddNumber(options,"Zona muerta izquierda (%)",config.LeftDead,0,40,v=>config.LeftDead=v);
        AddNumber(options,"Zona muerta derecha (%)",config.RightDead,0,40,v=>config.RightDead=v);
        AddNumber(options,"Recorrido para llegar al máximo (%)",config.Outer,60,100,v=>config.Outer=v);
        AddNumber(options,"Curva: 100 normal · >100 más suave al centro",config.Response,50,200,v=>config.Response=v);
        for(int i=0;i<4;i++) {
            int a=i;var cb=new CheckBox{Text="Invertir "+new[]{"X izquierda","Y izquierda","X derecha","Y derecha"}[i],AutoSize=true,Checked=config.Invert[i],Margin=new Padding(0,8,0,0)};
            cb.CheckedChanged+=(_,_)=>{config.Invert[a]=cb.Checked;AutoSave();};AddWide(options,cb);
        }
        var calibrate=new Button{Text="Calibrar centro (soltá las dos palancas)",AutoSize=true,Margin=new Padding(0,16,0,0)};
        calibrate.Click+=(_,_)=>{if(!engine.Connected)return;for(int i=0;i<4;i++)config.Centers[i]=SDL.SDL_GameControllerGetAxis(engine.Pad,i)/32768f;AutoSave();notice.Text="Centro calibrado para el mapa automático.";};AddWide(options,calibrate);
        var reset=new Button{Text="Restablecer centro",AutoSize=true,Margin=new Padding(0,8,0,0)};reset.Click+=(_,_)=>{Array.Clear(config.Centers);AutoSave();};AddWide(options,reset);
        AddWide(options,new Label{Text="Zona muerta radial: elimina movimientos cerca del centro.\nBajá el recorrido máximo si la palanca no llega al borde.\nLos gatillos del X5 Lite son digitales: LT y RT funcionan como botones.\nLa calibración de centro se aplica a los ejes automáticos. Al reasignar\nun eje se toma su posición de reposo durante la grabación.",AutoSize=true,Margin=new Padding(0,16,0,0)});
        var mapping=new TabPage("Botones y ejes"){Padding=new Padding(8)};tabs.TabPages.Add(mapping);
        grid.Dock=DockStyle.Fill;grid.AllowUserToAddRows=false;grid.AllowUserToDeleteRows=false;grid.RowHeadersVisible=false;grid.ReadOnly=true;grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;grid.MultiSelect=false;grid.BackgroundColor=Color.White;
        grid.Columns.Add("target","Control Xbox");grid.Columns.Add("binding","Asignación");
        grid.Columns.Add(new DataGridViewButtonColumn{HeaderText="",Text="Asignar",UseColumnTextForButtonValue=true,FillWeight=48});
        grid.Columns.Add(new DataGridViewButtonColumn{HeaderText="",Text="Automático",UseColumnTextForButtonValue=true,FillWeight=60});
        foreach(var n in Engine.Names)grid.Rows.Add(n,config.Bindings.TryGetValue(n,out var b)?b.ToString():"Mapa GameSir X5 Lite");
        foreach(DataGridViewColumn col in grid.Columns)col.SortMode=DataGridViewColumnSortMode.NotSortable;
        grid.CellContentClick+=(_,e)=>{if(e.RowIndex<0)return;string n=Engine.Names[e.RowIndex];if(e.ColumnIndex==2)BeginRecord(n);if(e.ColumnIndex==3){CancelRecord();config.Bindings.Remove(n);UpdateGrid();AutoSave();}};
        mapping.Controls.Add(grid);mapping.Controls.Add(new Label{Dock=DockStyle.Top,Height=68,Text="Asignar → soltá todo → presioná el botón o mové la palanca\nen la dirección indicada. Podés usar botones, ejes o cruceta.\nEsc cancela. La emulación se pausa mientras asignás."});
        root.Controls.Add(new Label{Text="Cerrá x360ce antes de activar. Dejá esta app abierta mientras jugás.\nSi un juego recibe dos mandos, revisá Steam Input o configurá HidHide.",Dock=DockStyle.Fill,Padding=new Padding(0,8,0,0)},0,4);
        Controls.Add(root);Controls.Add(notice);KeyPreview=true;KeyDown+=(_,e)=>{if(e.KeyCode==Keys.Escape)CancelRecord();};
        FormClosing+=(_,_)=>{timer.Stop();AutoSave();engine.Dispose();};timer.Tick+=(_,_)=>TickInput();ready=true;timer.Start();
    }
    static void AddWide(TableLayoutPanel p,Control c){int row=p.RowCount++;p.Controls.Add(c,0,row);p.SetColumnSpan(c,2);}
    void AddNumber(TableLayoutPanel p,string label,int val,int min,int max,Action<int> change) {
        int row=p.RowCount++;p.RowStyles.Add(new RowStyle(SizeType.Absolute,44));p.Controls.Add(new Label{Text=label,AutoSize=true,Anchor=AnchorStyles.Left},0,row);
        var num=new NumericUpDown{Value=val,Minimum=min,Maximum=max,Width=90,Anchor=AnchorStyles.Left};num.ValueChanged+=(_,_)=>{change((int)num.Value);AutoSave();};p.Controls.Add(num,1,row);
    }
    void Save(){try{config.Save();notice.Text="Ajustes guardados.";}catch(Exception ex){notice.Text="No se pudieron guardar: "+ex.Message;}}
    void AutoSave(){if(ready)Save();}
    void Toggle() {
        if(engine.Active)engine.Stop();else {
            if(System.Diagnostics.Process.GetProcessesByName("x360ce").Length>0){MessageBox.Show("Cerrá x360ce para evitar crear dos controles Xbox y volvé a activar.",Text);return;}
            try{engine.Start();}catch(Exception ex){MessageBox.Show("No se pudo activar Xbox.\n"+ex.Message+"\n\nEsta app usa el controlador ViGEmBus instalado en tu PC.",Text);}
        }
    }
    void BeginRecord(string name){if(!engine.Connected){notice.Text="Conectá el GameSir primero.";return;}engine.Stop();recording=name;baseline=Array.Empty<float>();recordAt=DateTime.UtcNow.AddSeconds(1);recordUntil=DateTime.UtcNow.AddSeconds(12);notice.Text=$"Soltá todo… después activá: {name}. Esc cancela.";}
    void CancelRecord(){recording=null;notice.Text="";}
    void UpdateGrid(){for(int i=0;i<Engine.Names.Length;i++)grid.Rows[i].Cells[1].Value=config.Bindings.TryGetValue(Engine.Names[i],out var b)?b.ToString():"Mapa GameSir X5 Lite";}
    void RecordTick() {
        if(recording==null)return;
        if(DateTime.UtcNow>recordUntil){CancelRecord();notice.Text="Tiempo agotado. Tocá Asignar para repetir.";return;}
        var axes=engine.Axes();var buttons=engine.RawButtons();var hats=engine.Hats();
        if(DateTime.UtcNow<recordAt || baseline.Length!=axes.Length){baseline=axes;baselineButtons=buttons;baselineHats=hats;return;}
        notice.Text=$"Ahora presioná o mové: {recording}. Esc cancela.";Binding? found=null;
        for(int i=0;i<buttons.Length;i++)if(buttons[i]!=0 && baselineButtons[i]==0){found=new("button",i,1,0);break;}
        if(found==null)for(int i=0;i<hats.Length;i++)if(hats[i]!=0 && hats[i]!=baselineHats[i]){int bit=new[]{1,2,4,8}.First(x=>(hats[i]&x)!=0);found=new("hat",i,bit,0);break;}
        if(found==null)for(int i=0;i<axes.Length;i++)if(Math.Abs(axes[i]-baseline[i])>.55f){found=new("axis",i,Math.Sign(axes[i]-baseline[i]),baseline[i]);break;}
        if(found!=null){string name=recording;config.Bindings[name]=found;recording=null;UpdateGrid();AutoSave();notice.Text=$"{name} → {found}. Volvé a activar Xbox cuando termines.";}
    }
    void TickInput() {
        try {
            if(ticks++%60==0)engine.Discover();engine.Read();
            if(!engine.Connected && recording!=null)CancelRecord();if(engine.Connected)RecordTick();engine.Submit();if(ticks%6!=0)return;
            status.Text=engine.Active?"● Xbox activo · listo para jugar":engine.Connected?"● GameSir X5 Lite conectado · podés probarlo y configurar":"○ Conectá el GameSir X5 Lite por USB";
            status.ForeColor=engine.Connected?Color.FromArgb(20,125,65):Color.FromArgb(120,90,30);
            toggle.Text=engine.Active?"Desactivar Xbox":"Activar control Xbox";toggle.Enabled=engine.Connected && recording==null;
            live.Text="Botones: "+string.Join("  ",Engine.Names.Take(17).Where((n,i)=>engine.Values[i]>.5f))+$"\nIzq. X {engine.LX:F2}  Y {engine.LY:F2}     Der. X {engine.RX:F2}  Y {engine.RY:F2}";
            preview.Values=engine.Values;preview.LX=engine.LX;preview.LY=engine.LY;preview.RX=engine.RX;preview.RY=engine.RY;preview.LeftDead=config.LeftDead;preview.RightDead=config.RightDead;preview.Recording=recording;
            mappingPrompt.Text=recording==null?"Hacé clic en el joystick para asignar un botón o una dirección.\nLos controles se iluminan al presionarlos. Clic derecho: restablecer.":$"Asignando: {recording} · {(DateTime.UtcNow<recordAt?"Soltá todos los controles…":"Ahora presioná el botón o mové la palanca.")}\nEsc cancela. Al terminar, volvé a activar Xbox.";
            mappingPrompt.BackColor=recording==null?Color.FromArgb(235,242,251):Color.FromArgb(255,239,208);preview.Invalidate();
        }catch(Exception ex){engine.Stop();notice.Text="Se detuvo la emulación: "+ex.Message;}
    }
}
