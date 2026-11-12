using System.Drawing.Drawing2D;

namespace GameSirSimple;

record ControlSpot(int Index,RectangleF Bounds,bool Round,string Label,Color Tint);

class ControllerView:Control
{
    public float[] Values=new float[25];
    public float LX,LY,RX,RY;
    public int LeftDead=10,RightDead=10;
    public string? Recording;
    public Func<string,string>? MappingText;
    public event Action<string>? AssignRequested;
    public event Action<string>? ResetRequested;
    int hovered=-1;
    readonly ToolTip tips=new(){InitialDelay=200,ReshowDelay=80,AutoPopDelay=12000};
    public static readonly ControlSpot[] Spots=CreateSpots();
    public ControllerView(){DoubleBuffered=true;ResizeRedraw=true;BackColor=Color.FromArgb(248,250,253);AccessibleName="Joystick interactivo: hacé clic para asignar botones y direcciones";}
    static ControlSpot[] CreateSpots()
    {
        var s=new List<ControlSpot>();
        void Rect(int n,float x,float y,float w,float h,string label)=>s.Add(new(n,new(x,y,w,h),false,label,Color.FromArgb(120,145,175)));
        void Circle(int n,float x,float y,float r,string label,Color? c=null)=>s.Add(new(n,new(x-r,y-r,r*2,r*2),true,label,c??Color.FromArgb(115,140,170)));
        Rect(15,91,42,133,36,"LT");Rect(9,81,87,153,33,"LB");
        Rect(16,676,42,133,36,"RT");Rect(10,666,87,153,33,"RB");
        Circle(4,363,236,23,"◀");Circle(5,450,236,25,"G");Circle(6,537,236,23,"▶");
        Circle(3,744,166,23,"Y",Color.FromArgb(234,185,63));Circle(0,744,270,23,"A",Color.FromArgb(54,192,149));
        Circle(2,692,218,23,"X",Color.FromArgb(71,153,241));Circle(1,796,218,23,"B",Color.FromArgb(232,99,119));
        Rect(11,244,295,32,34,"↑");Rect(12,244,363,32,34,"↓");Rect(13,210,329,34,32,"←");Rect(14,276,329,34,32,"→");
        void Stick(int center,int start,float x,float y,string name){
            Circle(center,x,y,23,name);Circle(start,x-48,y,17,"←");Circle(start+1,x+48,y,17,"→");Circle(start+2,x,y-48,17,"↑");Circle(start+3,x,y+48,17,"↓");
        }
        Stick(7,17,166,215,"L3");Stick(8,21,646,350,"R3");
        return s.ToArray();
    }
    public static RectangleF Viewport(Size size)
    {
        float scale=Math.Max(.01f,Math.Min(size.Width/900f,size.Height/500f));
        return new((size.Width-900*scale)/2,(size.Height-500*scale)/2,900*scale,500*scale);
    }
    public static int HitTest(Size size,Point point)
    {
        var v=Viewport(size);float x=(point.X-v.X)*900/v.Width,y=(point.Y-v.Y)*500/v.Height;
        foreach(var s in Spots){if(!s.Bounds.Contains(x,y))continue;if(s.Round){float dx=(x-(s.Bounds.X+s.Bounds.Width/2))/(s.Bounds.Width/2),dy=(y-(s.Bounds.Y+s.Bounds.Height/2))/(s.Bounds.Height/2);if(dx*dx+dy*dy>1)continue;}return s.Index;}
        return -1;
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);int hit=HitTest(ClientSize,e.Location);if(hit==hovered)return;hovered=hit;Cursor=hit>=0?Cursors.Hand:Cursors.Default;
        tips.SetToolTip(this,hit<0?"":$"{I18n.Name(Engine.Names[hit])} · {MappingText?.Invoke(Engine.Names[hit])??I18n.T("Mapa automático")}\n{I18n.T("Clic: asignar · Clic derecho: restablecer")}");Invalidate();
    }
    protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);hovered=-1;Invalidate();}
    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);int hit=HitTest(ClientSize,e.Location);if(hit<0)return;
        if(e.Button==MouseButtons.Left)AssignRequested?.Invoke(Engine.Names[hit]);
        if(e.Button==MouseButtons.Right){var menu=new ContextMenuStrip();menu.Items.Add(I18n.T("Restablecer {0} al mapa automático",I18n.Name(Engine.Names[hit])),null,(_,_)=>ResetRequested?.Invoke(Engine.Names[hit]));menu.Closed+=(_,_)=>menu.Dispose();menu.Show(this,e.Location);}
    }
    protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);Render(e.Graphics,ClientSize,Values,LX,LY,RX,RY,LeftDead,RightDead,Recording,hovered);}
    protected override void Dispose(bool disposing){if(disposing)tips.Dispose();base.Dispose(disposing);}
    static GraphicsPath Rounded(RectangleF r,float radius)
    {
        var p=new GraphicsPath();float d=radius*2;p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;
    }
    static void DrawText(Graphics g,string text,RectangleF rect,float size,Color c,FontStyle style=FontStyle.Regular)
    {using var f=new Font("Segoe UI",size,style,GraphicsUnit.Pixel);using var brush=new SolidBrush(c);using var sf=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center};g.DrawString(text,f,brush,rect,sf);}
    public static void Render(Graphics g,Size size,float[] values,float lx,float ly,float rx,float ry,int leftDead,int rightDead,string? recording=null,int hovered=-1)
    {
        var saved=g.Save();var view=Viewport(size);g.TranslateTransform(view.X,view.Y);g.ScaleTransform(view.Width/900,view.Height/500);g.SmoothingMode=SmoothingMode.AntiAlias;
        Color ink=Color.FromArgb(31,46,67),muted=Color.FromArgb(99,117,140);
        using(var shadow=Rounded(new(47,142,810,311),65))using(var b=new SolidBrush(Color.FromArgb(25,28,47,70)))g.FillPath(b,shadow);
        using(var rail=Rounded(new(150,151,600,252),36))using(var b=new LinearGradientBrush(new PointF(0,151),new PointF(0,403),Color.FromArgb(155,171,191),Color.FromArgb(76,92,114)))g.FillPath(b,rail);
        using(var phone=Rounded(new(318,166,264,226),20))using(var b=new SolidBrush(Color.FromArgb(235,240,247)))g.FillPath(b,phone);
        DrawText(g,"GAMESIR",new(330,278,240,35),25,Color.FromArgb(147,161,181),FontStyle.Bold);DrawText(g,"X5 LITE",new(330,311,240,26),14,Color.FromArgb(147,161,181));
        foreach(var rect in new[]{new RectangleF(46,130,271,310),new RectangleF(583,130,271,310)}){
            using var p=Rounded(rect,61);using var b=new LinearGradientBrush(rect,Color.FromArgb(49,65,86),Color.FromArgb(24,35,52),90);using var pen=new Pen(Color.FromArgb(69,87,110),2);g.FillPath(b,p);g.DrawPath(pen,p);
        }
        void Stick(float x,float y,float vx,float vy,int dead){
            using var ring=new Pen(Color.FromArgb(84,104,129),3);using var b=new SolidBrush(Color.FromArgb(14,24,38));g.FillEllipse(b,x-70,y-70,140,140);g.DrawEllipse(ring,x-70,y-70,140,140);
            using var dz=new SolidBrush(Color.FromArgb(60,106,142,176));float r=dead*.58f;g.FillEllipse(dz,x-r,y-r,r*2,r*2);
            using var cross=new Pen(Color.FromArgb(58,78,101),1);g.DrawLine(cross,x-60,y,x+60,y);g.DrawLine(cross,x,y-60,x,y+60);
            using var trace=new Pen(Color.FromArgb(70,190,220),3);g.DrawLine(trace,x,y,x+vx*36,y+vy*36);using var dot=new SolidBrush(Color.FromArgb(67,211,208));g.FillEllipse(dot,x+vx*36-5,y+vy*36-5,10,10);
        }
        Stick(166,215,lx,ly,leftDead);Stick(646,350,rx,ry,rightDead);
        using(var b=new SolidBrush(Color.FromArgb(15,25,40)))using(var p=Rounded(new(241,326,38,38),5))g.FillPath(b,p);
        foreach(var s in Spots){
            bool active=values[s.Index]>.5f,selected=recording==Engine.Names[s.Index],hover=s.Index==hovered;
            Color fill=selected?Color.FromArgb(232,149,40):active?Color.FromArgb(25,181,170):hover?Color.FromArgb(72,95,124):Color.FromArgb(35,49,69);
            Color stroke=selected?Color.FromArgb(255,206,113):active?Color.FromArgb(105,255,227):hover?Color.FromArgb(126,194,247):Color.FromArgb(92,112,139);
            using var b=new SolidBrush(fill);using var pen=new Pen(stroke,selected||active?3:1.5f);
            if(s.Round){g.FillEllipse(b,s.Bounds);g.DrawEllipse(pen,s.Bounds);}else{using var p=Rounded(s.Bounds,8);g.FillPath(b,p);g.DrawPath(pen,p);}
            DrawText(g,s.Label,s.Bounds,s.Index>=17?19: s.Index==7||s.Index==8?13:17,active||selected?Color.White:s.Tint,FontStyle.Bold);
        }
        DrawText(g,I18n.T("IZQUIERDA · ZONA MUERTA {0}%",leftDead),new(42,454,330,26),13,muted,FontStyle.Bold);DrawText(g,I18n.T("DERECHA · ZONA MUERTA {0}%",rightDead),new(528,454,330,26),13,muted,FontStyle.Bold);
        g.Restore(saved);
    }
}

