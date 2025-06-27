using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using DevComponents.AdvTree;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Controls;
using DevComponents.Editors;

namespace NoDev.Infinity.Binding
{
    internal static class NodeBinder
    {
        internal static void Bind(this NodeCollection e, object bindingClass)
        {
            var classType = bindingClass.GetType();

            var members = classType.GetMembers().Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property);

            foreach (var member in members)
            {
                var m = member;

                var attrs = member.GetCustomAttributes<BindingAttribute>();

                foreach (var attr in attrs)
                {
                    var info = member as FieldInfo;

                    var varType = info != null ? info.FieldType : ((PropertyInfo)member).PropertyType;

                    var varValue = GetValue(member, bindingClass);

                    if (attr is IntBindingAttribute)
                    {
                        var a = (IntBindingAttribute)attr;

                        var intVal = (int)Convert.ChangeType(varValue, typeof(int));

                        var maxValue = a.MaxValue;

                        if (maxValue == -1)
                        {
                            if (varValue is sbyte)
                                maxValue = sbyte.MaxValue;
                            else if (varValue is byte)
                                maxValue = byte.MaxValue;
                            else if (varValue is short)
                                maxValue = short.MaxValue;
                            else if (varValue is ushort)
                                maxValue = ushort.MaxValue;
                            else
                                maxValue = int.MaxValue;
                        }

                        e.InsertInt32Node(a.Text, intVal, maxValue, i => SetValue(m, bindingClass, Convert.ChangeType(i.Value, varType)), a.ShowMaxButton);
                    }
                    else if (attr is BoolBindingAttribute)
                    {
                        e.InsertCheckBoxNode(((BoolBindingAttribute)attr).Text, (bool)varValue, c => SetValue(m, bindingClass, c.Checked));
                    }
                    else if (attr is BitBindingAttribute)
                    {
                        var a = (BitBindingAttribute)attr;

                        e.InsertCheckBoxNode(a.Text, Convert.ToInt64(varValue).IsBitSet(a.Bit), c =>
                        {
                            var longVal = Convert.ToInt64(GetValue(m, bindingClass));
                            longVal = longVal.SetBit(a.Bit, c.Checked);
                            SetValue(m, bindingClass, Convert.ChangeType(longVal, varType));
                        });
                    }
                    else if (attr is IndexBindingAttribute)
                    {
                        var a = (IndexBindingAttribute)attr;

                        e.InsertComboBoxNode(a.Title, a.Items, Convert.ToInt32(varValue),
                            c => SetValue(m, bindingClass, Convert.ChangeType(c.SelectedIndex, varType)));
                    }
                    else if (attr is DecimalBindingAttribute)
                    {
                        var a = (DecimalBindingAttribute)attr;

                        var decVal = (decimal)Convert.ChangeType(varValue, typeof(decimal));

                        e.InsertDecimalNode(a.Text, decVal, a.MaxValue, a.DecimalPlaces, d => SetValue(m, bindingClass, Convert.ChangeType(d.Value, varType)));
                    }
                }
            }
        }

        private static object GetValue(MemberInfo info, object parent)
        {
            var fieldInfo = info as FieldInfo;

            return fieldInfo != null ? fieldInfo.GetValue(parent) : ((PropertyInfo)info).GetValue(parent);
        }

        private static void SetValue(MemberInfo info, object parent, object value)
        {
            var fieldInfo = info as FieldInfo;

            if (fieldInfo != null)
                fieldInfo.SetValue(parent, value);
            else
                ((PropertyInfo)info).SetValue(parent, value);
        }

        internal static Node InsertInt32Node(this NodeCollection e, string text, int value, int maxValue, Action<IntegerInput> onValueChange, bool showMaxButton)
        {
            var node = new Node(text);
            node.Cells.InsertInt32Cell(value, maxValue, onValueChange, showMaxButton);
            e.Add(node);
            return node;
        }

        internal static Cell InsertInt32Cell(this CellCollection cells, int value, int maxValue, Action<IntegerInput> onValueChange, bool showMaxButton)
        {
            var intInput = new IntegerInput {Value = value, MinValue = 0, MaxValue = maxValue, ShowUpDown = true};
            intInput.ValueChanged += (s, e) => onValueChange((IntegerInput)s);
            //intInput.Width = 100;

            if (showMaxButton)
            {
                intInput.ButtonCustom.Text = @"Max";
                intInput.ButtonCustomClick += (s, e) => { var i = (IntegerInput)s; i.Value = i.MaxValue; };
                intInput.ButtonCustom.Visible = true;
            }

            var cell = new Cell { HostedControl = intInput };
            cells.Add(cell);

            return cell;
        }

        internal static Node InsertDecimalNode(this NodeCollection e, string text, decimal value, decimal maxValue, int decimalPlaces, Action<NumericUpDown> onValueChange)
        {
            var node = new Node(text);
            node.Cells.InsertDecimalCell(value, maxValue, decimalPlaces, onValueChange);
            e.Add(node);
            return node;
        }

        internal static Cell InsertDecimalCell(this CellCollection cells, decimal value, decimal maxValue, int decimalPlaces, Action<NumericUpDown> onValueChange)
        {
            var numInput = new NumericUpDown
            {
                Value = value,
                Maximum = maxValue,
                Minimum = 0,
                DecimalPlaces = decimalPlaces,
                TextAlign = HorizontalAlignment.Right
            };
            numInput.ValueChanged += (s, e) => onValueChange((NumericUpDown)s);

            var cell = new Cell { HostedControl = numInput };
            cells.Add(cell);

            return cell;
        }

        internal static Node InsertCheckBoxNode(this NodeCollection e, string text, bool @checked, Action<CheckBoxItem> onCheckChange)
        {
            var ckInput = new CheckBoxItem {Text = text, Checked = @checked};
            ckInput.CheckedChanged += (s, ex) => onCheckChange((CheckBoxItem)s);

            var node = new Node { HostedItem = ckInput };
            e.Add(node);

            return node;
        }

        internal static Node InsertComboBoxNode(this NodeCollection e, string title, object[] items, int selectedIndex, Action<ComboBoxEx> onIndexChange)
        {
            var cbox = new ComboBoxEx
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Standard,
                FocusCuesEnabled = false
            };

            foreach (var i in items)
                cbox.Items.Add(i);

            cbox.SelectedItem = items[selectedIndex];
            cbox.SelectedIndexChanged += (s, ex) => onIndexChange((ComboBoxEx)s);

            var node = new Node(title);
            node.Cells.Add(new Cell { HostedControl = cbox });
            e.Add(node);

            return node;
        }

        internal static Cell InsertCheckBoxCell(this CellCollection cells, string text, bool @checked, Action<CheckBoxItem> onCheckChange)
        {
            var ckInput = new CheckBoxItem {Text = text, Checked = @checked};
            ckInput.CheckedChanged += (s, e) => onCheckChange((CheckBoxItem)s);

            var cell = new Cell { HostedItem = ckInput };
            cells.Add(cell);

            return cell;
        }
    }
}
