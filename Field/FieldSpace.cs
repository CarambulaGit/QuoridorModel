using System;
using Project.Interfaces;

namespace Project.Classes.Field {
    public class FieldSpace : ICloneable, ICanBeCrossed {
        public enum BlockType {
            Empty,
            Platform,
            Wall
        }

        public BlockType Type {
            get => _type;
            set {
                if (_type == value) return;
                _type = value;
                OnTypeChanged?.Invoke();
            }
        }

        private BlockType _type;
        public event Action OnTypeChanged;

        public FieldSpace(BlockType type = BlockType.Empty) {
            _type = type;
        }

        public override bool Equals(object obj) {
            if (!(obj is FieldSpace fieldSpace)) {
                return false;
            }

            return Type == fieldSpace.Type;
        }

        public bool CanBeCrossed() {
            return Type != BlockType.Wall;
        }

        public object Clone() {
            return new FieldSpace(Type);
        }

        // public event Action OnDestroy;

        // private void Destroy() {
        // OnDestroy?.Invoke();
        // UnsubscribeAllFromOnDestroy();
        // }

        // public void UnsubscribeAllFromOnDestroy() {
        // OnDestroy = null;
        // }
    }
}