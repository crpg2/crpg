using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Scripts
{
    // Token: 0x02000021 RID: 33
    internal class CrpgStandingPoint : StandingPoint
    {

        public UsableMachine? parentUsableMachine;

        // Token: 0x06000289 RID: 649 RVA: 0x0000D6A7 File Offset: 0x0000B8A7
        protected override void OnInit()
        {
            base.OnInit();
        }

        public UsableMachine? getParentUsableMachine()
        {
            return this.parentUsableMachine;
        }

        public void setParentUsableMachine(UsableMachine attachedUsableMachine)
        {
            this.parentUsableMachine = attachedUsableMachine;
        }
        

        // Token: 0x0600028A RID: 650 RVA: 0x0000D6B1 File Offset: 0x0000B8B1
    }
}
