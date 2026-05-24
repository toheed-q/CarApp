namespace DMF.Pages.Behaviors
{
    public class FocusNextEntryBehavior : Behavior<Entry>
    {
        public Entry? NextEntry { get; set; }
        protected override void OnAttachedTo(Entry bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.Completed += (_, _) => NextEntry?.Focus();
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.Completed -= (_, _) => NextEntry?.Focus();
        }
    }
}
