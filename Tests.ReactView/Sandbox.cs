﻿using System;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Tests.ReactView {

    internal class Sandbox : IDisposable {

        private TestReactView View { get; }

        public Sandbox(Window window, string propertyValue) : this(propertyValue) {
            AttachTo(window);
        }

        public Sandbox(string propertyValue) {
            View = new TestReactView {
                PropertyValue = propertyValue
            };
        }

        public void AttachTo(Window window) {
            window.Content = View;
        }

        public string GetFirstRenderHtml() {
            return View.EvaluateMethod<string>("getFirstRenderHtml");
        }

        public string GetHtml() {
            return View.EvaluateMethod<string>("getHtml");
        }

        public string GetPropertyValue() {
            return View.EvaluateMethod<string>("getPropertyValue");
        }

        public string PropertyValue { 
            get => View.PropertyValue; 
            set => View.PropertyValue = value; 
        }

        public async Task Ready() {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            View.Ready += delegate {
                taskCompletionSource.SetResult(true);
            };
            await taskCompletionSource.Task;
        }

        public void Dispose() {
            View.Dispose();
        }
    }
}