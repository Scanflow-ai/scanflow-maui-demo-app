using ScanflowMauiDemoApp.Helpers;
using ScanflowMauiDemoApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanflowMauiDemoApp.ViewModels
{
    public class HomePageViewModel
    {
        public ObservableCollection<ScanSelect> ScanModels { get; set; } = new ObservableCollection<ScanSelect>
       {
                new ScanSelect
                {
                    Name= ConstantStrings.QR_Code,
                    Image="qrcode"
                }, new ScanSelect
                {
                    Name=ConstantStrings.Barcode,
                    Image="barcode"
                }, new ScanSelect
                {
                    Name=ConstantStrings.Any,
                    Image="any"
                }, new ScanSelect
                {
                    Name=ConstantStrings.Batch,
                    Image="batch"
                }, new ScanSelect
                {
                    Name=ConstantStrings.ManyCode,
                    Image="manycode"
                }, new ScanSelect
                {
                    Name=ConstantStrings.Pivot,
                    Image="pivot"
                }, new ScanSelect
                {
                    Name=ConstantStrings.IDcard,
                    Image="qrcode"
                }, new ScanSelect
                {
                    Name=ConstantStrings.Tyre,
                    Image="qrcode"
                }, new ScanSelect
                {
                    Name=ConstantStrings.Vertical,
                    Image="qrcode"
                },
                new ScanSelect
                {
                    Name=ConstantStrings.Horizontal,
                   Image="qrcode"
                },
       };
        public HomePageViewModel()
        {

        }
    }
}

