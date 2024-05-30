using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using InventoryEngine.Things;

namespace InventoryEngine
{
    public interface IInventory
    {
        void AddListener(ObservingData.OnChangedDelegate listener);

        void RemoveListener(ObservingData.OnChangedDelegate listener);

        Slote SloteAt(int index);

        void AddSlotes(int amount);

        void RemoveSlotes(int amount);

        bool IsFittingTheType(Thing thing);

        bool CanAdd(Thing thing, int amount);

        bool CanAdd(Dictionary<Thing, int> things);

        /// <summary> Returns the amount of things that can't be added because inventory is full </summary> 
        int Add(Item newitem, int amount);

        /// <summary> Returns the amount of things that can't be added because inventory is full </summary> 
        int AddToSlote(int sloteid, Item newitem, int amount);

        bool Contains(Thing thing, int amount);

        /// <summary> Clears inventory without handling the item destruct </summary> 
        void ClearAll();

        int Size { get; }

        Slote GetFirstEmptySlote();

        int GetAmountOf(Thing thing);

        /// <summary> Returns the amount of things that can't be removed because there are no such amount of things </summary> 
        int Remove(Thing newthing, int amount);

        object GetSerializedData();

        void SetSerializedData(object SerializedData);
    }

    public class ObservingData
    {
        private ObservingData() { }
        public delegate void OnChangedDelegate();
    }

    public class Inventory<T> : IInventory where T : ThingProperty
    {

        public event ObservingData.OnChangedDelegate OnChanged;

        private List<Slote> _slotes;

        public Inventory(int size)
        {
            _slotes = new List<Slote>();
            for(int i = 0;i < size;i++)
            {
                _slotes.Add(new Slote(this));
                _slotes[_slotes.Count - 1].OnSloteChanged += OnSloteChanged;
            }
        }

        public void AddSlotes(int amount)
        {
            for(int i = 0; i < amount;i++)
            {
                _slotes.Add(new Slote(this));
                _slotes[_slotes.Count - 1].OnSloteChanged += OnSloteChanged;
            }
            OnChanged?.Invoke();
        }

        public void RemoveSlotes(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                _slotes.RemoveAt(_slotes.Count - 1);
            }
            OnChanged?.Invoke();
        }

        public void AddListener(ObservingData.OnChangedDelegate listener)
        {
            OnChanged += listener;
        }

        public void RemoveListener(ObservingData.OnChangedDelegate listener)
        {
            OnChanged -= listener;
        }

        public Slote SloteAt(int index)
        {
            if (index < 0 || index >= Size)
            {
                Debug.LogWarning($"[GettingSlote]: You are trying to get slote at {index} but the slote range is 0 - {Size - 1} (inclusive)");
                return null;
            }
            return _slotes[index];
        }

        public bool IsFittingTheType(Thing thing)
        {
            return typeof(T) == typeof(ThingProperty) || thing.GetPropertyofType<T>() != null;
        }

        public bool CanAdd(Thing thing, int amount)
        {
            Dictionary<Thing, int> onething = new Dictionary<Thing, int>();
            onething.Add(thing, amount);
            return CanAdd(onething);
        }

        public bool CanAdd(Dictionary<Thing, int> things)
        {
            foreach (var thing in things)
            {
                if(IsFittingTheType(thing.Key))
                {
                    int amount = thing.Value;
                    for(int i = 0;i < _slotes.Count;i++)
                    {
                        if(!_slotes[i].IsEmpty)
                        {
                            if (_slotes[i].Item.Thing == thing.Key)
                            {
                                amount -= thing.Key.MaxAmountInSlote - _slotes[i].Amount;
                                if (amount <= 0)
                                    break;
                            }
                        }
                        else
                        {
                            if (thing.Key.MaxAmountInSlote > amount)
                            {
                                amount = 0;
                                break;
                            }
                            else
                                amount -= thing.Key.MaxAmountInSlote;
                        }
                    }
                    if (amount > 0)
                        return false;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary> Returns the amount of things that can't be added because inventory is full </summary> 
        public int Add(Item newitem, int amount)
        {
            if (newitem != null)
            {
                if (!IsFittingTheType(newitem.Thing))
                {
                    return amount;
                }
                List<int> emptyslotes = new List<int>();
                for (int i = 0; i < _slotes.Count; i++)
                {
                    if (!_slotes[i].IsEmpty)
                    {
                        if (_slotes[i].Item.Thing == newitem.Thing)
                        {
                            if (newitem.Thing.MaxAmountInSlote - _slotes[i].Amount >= amount)
                            {
                                _slotes[i].Amount += amount;
                                OnChanged?.Invoke();
                                return 0;
                            }
                            else
                            {
                                amount -= newitem.Thing.MaxAmountInSlote - _slotes[i].Amount;
                                _slotes[i].Amount = newitem.Thing.MaxAmountInSlote;
                            }
                        }
                    }
                    else
                    {
                        emptyslotes.Add(i);
                    }
                }
                if (amount > 0)
                {
                    for (int i = 0; i < emptyslotes.Count; i++)
                    {
                        if (newitem.Thing.MaxAmountInSlote >= amount)
                        {
                            _slotes[emptyslotes[i]].SetItem(newitem, amount);
                            return 0;
                        }
                        else
                        {
                            amount -= newitem.Thing.MaxAmountInSlote;
                            _slotes[emptyslotes[i]].SetItem(Item.Create(newitem.Thing), newitem.Thing.MaxAmountInSlote);
                        }
                    }
                }
            }
            else
            {
                return 0;
            }
            return amount;
        }

        /// <summary> Returns the amount of things that can't be added because inventory is full </summary> 
        public int AddToSlote(int sloteid, Item newitem, int amount)
        {
            if (newitem != null)
            {
                if (!IsFittingTheType(newitem.Thing))
                {
                    return amount;
                }
                if (_slotes[sloteid].IsEmpty)
                {
                    if (newitem.Thing.MaxAmountInSlote >= amount)
                    {
                        _slotes[sloteid].SetItem(newitem, amount);
                        return 0;
                    }
                    else
                    {
                        _slotes[sloteid].SetItem(newitem, newitem.Thing.MaxAmountInSlote);
                        return amount - newitem.Thing.MaxAmountInSlote;
                    }
                }
                else
                {
                    if (_slotes[sloteid].Item.Thing == newitem.Thing)
                    {
                        if (newitem.Thing.MaxAmountInSlote - _slotes[sloteid].Amount >= amount)
                        {
                            _slotes[sloteid].Amount += amount;
                            return 0;
                        }
                        else
                        {
                            int sloteamount = _slotes[sloteid].Amount;
                            _slotes[sloteid].Amount = newitem.Thing.MaxAmountInSlote;
                            return amount - (newitem.Thing.MaxAmountInSlote - sloteamount);
                        }
                    }
                }
            }
            else
            {
                return 0;
            }
            return amount;
        }

        public bool Contains(Thing thing, int amount)
        {
            for(int i = 0; i < _slotes.Count;i++)
            {
                if (!_slotes[i].IsEmpty)
                {
                    if (_slotes[i].Item.Thing == thing)
                    {
                        if (_slotes[i].Amount >= amount)
                        {
                            return true;
                        }
                        else
                        {
                            amount -= _slotes[i].Amount;
                        }
                    }
                }
            }
            return false;
        }


        public void ClearAll()
        {
            for(int i = 0; i < _slotes.Count;i++)
            {
                _slotes[i].Amount = 0;
            }
        }

        public int Size => _slotes.Count;

        public Slote GetFirstEmptySlote()
        {
            for(int i = 0; i < _slotes.Count;i++)
            {
                if (_slotes[i].IsEmpty)
                    return _slotes[i];
            }
            return null;
        }

        public int GetAmountOf(Thing thing)
        {
            int amount = 0;
            for (int i = 0; i < _slotes.Count; i++)
            {
                if (!_slotes[i].IsEmpty)
                {
                    if (_slotes[i].Item.Thing == thing)
                    {
                        amount += _slotes[i].Amount;
                    }
                }
            }
            return amount;
        }

        /// <summary> Returns the amount of things that can't be removed because there are no such amount of things </summary> 
        public int Remove(Thing newthing, int amount)
        {
            bool changed = false;
            for (int i = 0; i < _slotes.Count; i++)
            {
                if (!_slotes[i].IsEmpty)
                {
                    if (_slotes[i].Item.Thing == newthing)
                    {
                        if (_slotes[i].Amount >= amount)
                        {
                            _slotes[i].Amount -= amount;
                            OnChanged?.Invoke();
                            return 0;
                        }
                        else
                        {
                            amount -= _slotes[i].Amount;
                            _slotes[i].Amount = 0;
                            changed = true;
                        }
                    }
                }
            }
            if (changed)
                OnChanged?.Invoke();
            return amount;
        }

        public object GetSerializedData()
        {
            object[] data = new object[Size];
            for(int i = 0; i < Size;i++)
            {
                Dictionary<string, object> slotedata = new Dictionary<string, object>();
                slotedata.Add("amount", _slotes[i].Amount);
                if (!_slotes[i].IsEmpty)
                    slotedata.Add("item", _slotes[i].Item.GetSerializedData());
                else
                    slotedata.Add("item", null);
                data[i] = slotedata;
            }
            return data;
        }

        public void SetSerializedData(object SerializedData)
        {
            if (SerializedData == null)
                return;
            _slotes.Clear();
            object[] data = (object[])SerializedData;
            for(int i = 0; i < data.Length;i++)
            {
                Dictionary<string, object> slotedata = (Dictionary<string, object>)data[i];
                AddSlotes(1);
                if ((int)slotedata["amount"] > 0)
                    _slotes[_slotes.Count - 1].SetItem(Item.Create(slotedata["item"]), (int)slotedata["amount"]);
            }
        }

        private void OnSloteChanged(Slote slote)
        {
            OnChanged?.Invoke();
        }
    }

    public class Slote
    {
        public delegate void OnSloteChangedDelegate(Slote slote);

        public event OnSloteChangedDelegate OnSloteChanged;

        public Slote(IInventory parent)
        {
            if (parent == null)
                throw new ArgumentNullException("[Slote.ctor]: Creating slote: 'parent' argument is null!");
            this._parent = parent;
        }

        private int _amount = 0;

        private Item _item;

        private IInventory _parent;

        public IInventory Inventory => _parent;

        public Item Item => _item;

        public void SetItem(Item newitem, int newamount)
        {
            if(newamount > 0 && newitem != null)
            {
                if (_parent.IsFittingTheType(newitem.Thing))
                {
                    _item = newitem;
                    _item.OnDestroyItemEvent += OnDestroyItem;
                    Amount = newamount;
                }
                else
                {
                    Debug.LogWarning($"[ItemAssignment]: You are trying to set item (thing: {_item.Thing.Name}) that is typically incompatible with inventory type! Assignment is cancelled");
                    return;
                }
            }
            else
            {
                if(_item != null)
                {
                    _item.OnDestroyItemEvent -= OnDestroyItem;
                }
                _item = null;
                _amount = 0;
                OnSloteChanged?.Invoke(this);
            }
        }

        public int Amount
        {
            get { return _amount; }
            set
            {
                if (value == 0)
                {
                    _amount = 0;
                    if (_item != null)
                    {
                        _item.OnDestroyItemEvent -= OnDestroyItem;
                    }
                    _item = null;
                }
                else
                {
                    if (_item != null)
                        _amount = Mathf.Clamp(value, 0, _item.Thing.MaxAmountInSlote);
                    else
                        Debug.LogWarning("[Amount.set] You are trying to add some amount to slote, but slote is empty ('item' variable is null) !");
                    return;
                }
                OnSloteChanged?.Invoke(this);
            }
        }

        public bool IsEmpty => _amount == 0;

        public void ExchangeInfoWithAnotherSlote(Slote anotherslote)
        {
            if (IsEmpty && anotherslote.IsEmpty)
                return;

            if (!IsEmpty)
            {
                if (!anotherslote.Inventory.IsFittingTheType(_item.Thing))
                {
                    Debug.LogWarning($"[ExchangeOperation]: You are trying to set item (thing: {_item.Thing.Name}) that is typically incompatible with inventory type! Assignment is cancelled");
                    return;
                }
            }

            if (!anotherslote.IsEmpty)
            {
                if (!_parent.IsFittingTheType(anotherslote._item.Thing))
                {
                    Debug.LogWarning($"[ExchangeOperation]: You are trying to set item (thing: {_item.Thing.Name}) that is typically incompatible with inventory type! Assignment is cancelled");
                    return;
                }
            }

            int myamount = _amount;
            Item myitem = _item;

            if (myitem != null)
                myitem.OnDestroyItemEvent -= OnDestroyItem;
            if (anotherslote._item != null)
                anotherslote._item.OnDestroyItemEvent -= anotherslote.OnDestroyItem;

            _amount = anotherslote._amount;
            _item = anotherslote._item;

            anotherslote._amount = myamount;
            anotherslote._item = myitem;

            if (_item != null)
                _item.OnDestroyItemEvent += OnDestroyItem;
            if (anotherslote._item != null)
                anotherslote._item.OnDestroyItemEvent += anotherslote.OnDestroyItem;

            OnSloteChanged?.Invoke(this);
            anotherslote.OnSloteChanged?.Invoke(anotherslote);
        }

        private void OnDestroyItem()
        {          
            Amount--;
        }

        public override string ToString()
        {
            if (!IsEmpty)
                return $"Slote info \n   Content: {_item.Thing.Name} \n   Amount: {_amount} \n";
            else
                return $"Slote info \n   Empty";
        }
    }
}
