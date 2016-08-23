using System;
using System.Linq;

namespace Evade.Obstacles.Particles
{
    using Ensage;
    using Ensage.Common.Extensions;

    using SharpDX;

    public sealed class ObstacleParticleHook : ObstacleParticle
    {
        /*
         * Control Points
         * 0 == StartPosition
         * 1 == EndPosition
         */
        public ObstacleParticleHook( NavMeshPathfinding pathfinding, Entity owner, ParticleEffect particleEffect)
            : base(0, owner, particleEffect)
        {
            var ability =
                ObjectManager.GetEntities<Ability>()
                    .FirstOrDefault(x => x.ClassID == ClassID.CDOTA_Ability_Pudge_MeatHook);
            if (ability?.Owner.Team == ObjectManager.LocalHero.Team)
                throw new Exception();

            Radius = ability?.GetRadius(ability.Name) + 8 ?? 108;
            if (ability != null && ability.Level > 0)
            {
                _range = ability.GetRange(ability.Level - 1) + Radius;
            }
            Radius /= 2; // Left and right is only halfs
            
            var special = ability?.AbilitySpecialData.FirstOrDefault(x => x.Name == "hook_speed");
            if (special != null)
            {
                _speed = special.Value;
            }

            ID = pathfinding.AddObstacle(Position, EndPosition, Radius);
            Debugging.WriteLine("Adding Hook particle: {0} - {1}", Radius, _range);
        }

        private readonly float _range = 700;
        private readonly float _speed = 1600;
        public override bool IsLine => true;

        public override Vector3 Position => ParticleEffect.GetControlPoint(0);

        public override Vector3 EndPosition
        {
            get
            {
                var result = ParticleEffect.GetControlPoint(1);
                var direction = result - Position;
                direction.Normalize();
                direction *= Radius;
                return result + direction;
            }
        }

        public override Vector3 CurrentPosition
        {
            get
            {
                var result = Position;
                var end = EndPosition;
                var direction = end - result;
                direction.Normalize();
                direction *= _speed * (Game.RawGameTime - Started);
                result += direction;
                return result;
            }
        }

        public override float Radius { get; }

        public override float TimeLeft => Math.Max(0, (Started + _range/_speed) - Game.RawGameTime);

        public override bool IsValid => base.IsValid && TimeLeft > 0.1;
    }
}
