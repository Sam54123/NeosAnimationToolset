using FrooxEngine;

namespace NeosAnimationToolset
{
    [Category("World")]
    public partial class userList : Component
    {
        public readonly SyncRef<Slot> template;
        public readonly Sync<bool> excludeHost;
        private Slot lastTemplate;
        private void addUser(User user)
        {
            if(excludeHost ? !user.IsHost : true)
            {
                template.Target.Duplicate(this.Slot).Name = user.UserID;
            }
        }
        private void removeUser(User user)
        {
            Slot.FindChild(s => { return s.Name == user.UserID; }).Destroy();
        }
        protected override void OnChanges()
        {
            if (template.Target != lastTemplate && template.Target != null)
            {
                lastTemplate = template.Target;
                Slot.DestroyChildren();
                foreach (User user in this.World.AllUsers)
                {
                    addUser(user);
                }
            }
            base.OnChanges();
        }
        protected override void OnAttach()
        {
            lastTemplate = template.Target;

            excludeHost.OnValueChange += (SyncField<bool> syncField) => {
                if (excludeHost.Value) removeUser(World.HostUser);
                else addUser(World.HostUser);
            };
        }


        public override void OnUserJoined(User user)
        {
            addUser(user);
        }
        public override void OnUserLeft(User user)
        {
            removeUser(user);
        }
    }
}
