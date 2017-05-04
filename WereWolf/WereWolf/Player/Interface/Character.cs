﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WereWolf
{
    public interface Character
    {
        bool isNightPlayer();
        bool isDead();
        bool canHeal();
        bool canQuestion();
        bool canKill();
        void kill();
        void heal();
    }
}
