using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ThingEngine;
using System;

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

        private event ObservingData.OnChangedDelegate OnChangedEvent;

        public void AddListener(ObservingData.OnChangedDelegate listener)
        {
            OnChangedEvent += listener;
        }

        public void RemoveListener(ObservingData.OnChangedDelegate listener)
        {
            OnChangedEvent -= listener;
        }

        public Slote SloteAt(int index)
        {
            if (index < 0 || index >= Size)
            {
                throw new ArgumentOutOfRangeException($"You are trying to get slote at {index} but the slote range is 0 - {Size - 1} (inclusive)");
            }
            return slotes[index];
        }

        private List<Slote> slotes;

        public Inventory(int size)
        {
            slotes = new List<Slote>();
            for(int i = 0;i < size;i++)
            {
                slotes.Add(new Slote());
                slotes[slotes.Count - 1].OnSloteChangedEvent += OnSloteChanged;
            }
        }

        public void AddSlotes(int amount)
        {
            for(int i = 0; i < amount;i++)
            {
                slotes.Add(new Slote());
                slotes[slotes.Count - 1].OnSloteChangedEvent += OnSloteChanged;
            }
            OnChangedEvent?.Invoke();
        }

        public void RemoveSlotes(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                slotes.RemoveAt(slotes.Count - 1);
            }
            OnChangedEvent?.Invoke();
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
                    for(int i = 0;i < slotes.Count;i++)
                    {
                        if(!slotes[i].IsEmpty)
                        {
                            if (slotes[i].item.thing == thing.Key)
                            {
                                amount -= thing.Key.MaxAmountInSlote - slotes[i].Amount;
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
                if (!IsFittingTheType(newitem.thing))
                {
                    return amount;
                }
                List<int> emptyslotes = new List<int>();
                for (int i = 0; i < slotes.Count; i++)
                {
                    if (!slotes[i].IsEmpty)
                    {
                        if (slotes[i].item.thing == newitem.thing)
                        {
                            if (newitem.thing.MaxAmountInSlote - slotes[i].Amount >= amount)
                            {
                                slotes[i].Amount += amount;
                                OnChangedEvent?.Invoke();
                                return 0;
                            }
                            else
                            {
                                amount -= newitem.thing.MaxAmountInSlote - slotes[i].Amount;
                                slotes[i].Amount = newitem.thing.MaxAmountInSlote;
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
                        if (newitem.thing.MaxAmountInSlote >= amount)
                        {
                            slotes[emptyslotes[i]].SetItem(newitem, amount);
                            return 0;
                        }
                        else
                        {
                            amount -= newitem.thing.MaxAmountInSlote;
                            slotes[emptyslotes[i]].SetItem(Item.Create(newitem.thing), newitem.thing.MaxAmountInSlote);
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
                if (!IsFittingTheType(newitem.thing))
                {
                    return amount;
                }
                if (slotes[sloteid].IsEmpty)
                {
                    if (newitem.thing.MaxAmountInSlote >= amount)
                    {
                        slotes[sloteid].SetItem(newitem, amount);
                        return 0;
                    }
                    else
                    {
                        slotes[sloteid].SetItem(newitem, newitem.thing.MaxAmountInSlote);
                        return amount - newitem.thing.MaxAmountInSlote;
                    }
                }
                else
                {
                    if (slotes[sloteid].item.thing == newitem.thing)
                    {
                        if (newitem.thing.MaxAmountInSlote - slotes[sloteid].Amount >= amount)
                        {
                            slotes[sloteid].Amount += amount;
                            return 0;
                        }
                        else
                        {
                            int sloteamount = slotes[sloteid].Amount;
                            slotes[sloteid].Amount = newitem.thing.MaxAmountInSlote;
                            return amount - (newitem.thing.MaxAmountInSlote - sloteamount);
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
            for(int i = 0; i < slotes.Count;i++)
            {
                if (!slotes[i].IsEmpty)
                {
                    if (slotes[i].item.thing == thing)
                    {
                        if (slotes[i].Amount >= amount)
                        {
                            return true;
                        }
                        else
                        {
                            amount -= slotes[i].Amount;
                        }
                    }
                }
            }
            return false;
        }


        public void ClearAll()
        {
            for(int i = 0; i < slotes.Count;i++)
            {
                slotes[i].Amount = 0;
            }
        }

        public int Size
        {
            get { return slotes.Count; }
        }

        public Slote GetFirstEmptySlote()
        {
            for(int i = 0; i < slotes.Count;i++)
            {
                if (slotes[i].IsEmpty)
                    return slotes[i];
            }
            return null;
        }

        public int GetAmountOf(Thing thing)
        {
            int amount = 0;
            for (int i = 0; i < slotes.Count; i++)
            {
                if (!slotes[i].IsEmpty)
                {
                    if (slotes[i].item.thing == thing)
                    {
                        amount += slotes[i].Amount;
                    }
                }
            }
            return amount;
        }

        /// <summary> Returns the amount of things that can't be removed because there are no such amount of things </summary> 
        public int Remove(Thing newthing, int amount)
        {
            bool changed = false;
            for (int i = 0; i < slotes.Count; i++)
            {
                if (!slotes[i].IsEmpty)
                {
                    if (slotes[i].item.thing == newthing)
                    {
                        if (slotes[i].Amount >= amount)
                        {
                            slotes[i].Amount -= amount;
                            OnChangedEvent?.Invoke();
                            return 0;
                        }
                        else
                        {
                            amount -= slotes[i].Amount;
                            slotes[i].Amount = 0;
                            changed = true;
                        }
                    }
                }
            }
            if (changed)
                OnChangedEvent?.Invoke();
            return amount;
        }

        public object GetSerializedData()
        {
            object[] data = new object[Size];
            for(int i = 0; i < Size;i++)
            {
                Dictionary<string, object> slotedata = new Dictionary<string, object>();
                slotedata.Add("amount", slotes[i].Amount);
                if (!slotes[i].IsEmpty)
                    slotedata.Add("item", slotes[i].item.GetSerializedData());
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
            slotes.Clear();
            object[] data = (object[])SerializedData;
            for(int i = 0; i < data.Length;i++)
            {
                Dictionary<string, object> slotedata = (Dictionary<string, object>)data[i];
                AddSlotes(1);
                if ((int)slotedata["amount"] > 0)
                    slotes[slotes.Count - 1].SetItem(Item.Create(slotedata["item"]), (int)slotedata["amount"]);
            }
        }

        private void OnSloteChanged(Slote slote)
        {
            OnChangedEvent?.Invoke();
        }


    }

    public class Slote
    {
        public delegate void OnSloteChangedDelegate(Slote slote);

        public event OnSloteChangedDelegate OnSloteChangedEvent;

        private int amount = 0;

        public Item item { get; private set; }

        public void SetItem(Item newitem, int newamount)
        {
            if(newamount > 0 && newitem != null)
            {
                item = newitem;
                item.OnDestroyItemEvent += OnDestroyItem;
                Amount = newamount;
            }
            else
            {
                item = null;
                amount = 0;
                OnSloteChangedEvent?.Invoke(this);
            }
        }

        public int Amount
        {
            get { return amount; }
            set
            {
                if (value == 0)
                {
                    amount = 0;
                    if (item != null)
                    {
                        item.OnDestroyItemEvent -= OnDestroyItem;
                    }
                    item = null;
                }
                else
                {
                    if (item != null)
                        amount = Mathf.Clamp(value, 0, item.thing.MaxAmountInSlote);
                    else
                        throw new Exception("You are trying to add some amount to slote, but slote is empty ('item' variable is null) !");
                }
                OnSloteChangedEvent?.Invoke(this);
            }
        }

        public bool IsEmpty { get { return amount == 0; } }

        public void ExchangeInfoWithAnotherSlote(Slote anotherslote)
        {
            int myamount = amount;
            Item myitem = item;

            if (myitem != null)
                myitem.OnDestroyItemEvent -= OnDestroyItem;
            if (anotherslote.item != null)
                anotherslote.item.OnDestroyItemEvent -= anotherslote.OnDestroyItem;

            amount = anotherslote.amount;
            item = anotherslote.item;

            anotherslote.amount = myamount;
            anotherslote.item = myitem;

            if (item != null)
                item.OnDestroyItemEvent += OnDestroyItem;
            if (anotherslote.item != null)
                anotherslote.item.OnDestroyItemEvent += anotherslote.OnDestroyItem;

            OnSloteChangedEvent?.Invoke(this);
            anotherslote.OnSloteChangedEvent?.Invoke(anotherslote);
        }

        private void OnDestroyItem()
        {          
            Amount--;
        }

        public override string ToString()
        {
            if (!IsEmpty)
                return $"Slote info \n   Content: {item.thing.Name} \n   Amount: {amount} \n";
            else
                return $"Slote info \n   Empty";
        }
    }
}
