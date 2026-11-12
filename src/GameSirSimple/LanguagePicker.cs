namespace GameSirSimple;

// A dropdown button keeps native language names visible in every UI language.
sealed class LanguagePicker:Button
{
    public sealed record Choice(string Code,string Label);
    static readonly Choice[] Choices={new("es","Español"),new("en","English"),new("zh-CN","简体中文")};
    int index;
    public Choice SelectedItem=>Choices[index];
    public event EventHandler? SelectedIndexChanged;
    public int SelectedIndex{
        get=>index;
        set{if(value<0 || value>=Choices.Length)throw new ArgumentOutOfRangeException(nameof(value));bool changed=value!=index;index=value;Text=SelectedItem.Label+" ▾";if(changed)SelectedIndexChanged?.Invoke(this,EventArgs.Empty);}
    }
    public LanguagePicker(){Text=Choices[0].Label+" ▾";TextAlign=ContentAlignment.MiddleLeft;AccessibleName="Español / English / 简体中文";}
    protected override void OnClick(EventArgs e){
        base.OnClick(e);var menu=new ContextMenuStrip();
        for(int i=0;i<Choices.Length;i++){int target=i;var item=new ToolStripMenuItem(Choices[i].Label){Checked=index==i};item.Click+=(_,_)=>SelectedIndex=target;menu.Items.Add(item);}
        menu.Closed+=(_,_)=>menu.Dispose();menu.Show(this,new Point(0,Height));
    }
}
