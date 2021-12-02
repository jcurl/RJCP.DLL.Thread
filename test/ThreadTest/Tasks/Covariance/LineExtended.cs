namespace RJCP.Threading.Tasks.Covariance
{
    public class LineExtended : Line
    {
        public override string Text
        {
            get { return "Line"; }
        }

        public virtual int Line
        {
            get { return 0; }
        }
    }
}
