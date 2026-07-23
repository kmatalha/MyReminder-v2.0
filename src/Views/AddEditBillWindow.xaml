<Window x:Class="RemindMe.Views.AddEditBillWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Bill" Height="700" Width="420" WindowStartupLocation="CenterOwner"
        Background="{DynamicResource AppBackgroundBrush}" FontFamily="{StaticResource AppFont}"
        ResizeMode="NoResize">
    <ScrollViewer Padding="24" VerticalScrollBarVisibility="Auto">
        <StackPanel>
            <TextBlock x:Name="TitleText" Text="Add Bill" Style="{StaticResource HeadingText}" FontSize="20" Margin="0,0,0,20"/>

            <TextBlock Text="Name" Style="{StaticResource SubText}" Margin="0,0,0,4"/>
            <TextBox x:Name="NameBox" Padding="8" Margin="0,0,0,14"/>

            <TextBlock Text="Category" Style="{StaticResource SubText}" Margin="0,0,0,4"/>
            <ComboBox x:Name="CategoryCombo" Margin="0,0,0,14">
                <ComboBoxItem Content="Utility"/>
                <ComboBoxItem Content="Subscription"/>
                <ComboBoxItem Content="Fee"/>
                <ComboBoxItem Content="Rent"/>
                <ComboBoxItem Content="Insurance"/>
                <ComboBoxItem Content="Loan"/>
                <ComboBoxItem Content="Other"/>
            </ComboBox>

            <TextBlock Text="Description (optional)" Style="{StaticResource SubText}" Margin="0,0,0,4"/>
            <TextBox x:Name="DescriptionBox" Padding="8" Margin="0,0,0,14" Height="64"
                     AcceptsReturn="True" TextWrapping="Wrap" VerticalScrollBarVisibility="Auto"/>

            <TextBlock Text="Due Date" Style="{StaticResource SubText}" Margin="0,0,0,4"/>
            <DatePicker x:Name="DueDatePicker" Margin="0,0,0,14"/>

            <TextBlock Text="Reminder Start Date" Style="{StaticResource SubText}" Margin="0,0,0,4"/>
            <DatePicker x:Name="ReminderStartPicker" Margin="0,0,0,14"/>

            <TextBlock Text="Alarm Time (when the reminder rings)" Style="{StaticResource SubText}" Margin="0,0,0,4"/>
            <Grid Margin="0,0,0,14">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="16"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                <ComboBox x:Name="ReminderHourCombo" Grid.Column="0"/>
                <TextBlock Grid.Column="1" Text=":" FontSize="16" HorizontalAlignment="Center"
                           VerticalAlignment="Center" Style="{StaticResource SubText}"/>
                <ComboBox x:Name="ReminderMinuteCombo" Grid.Column="2"/>
            </Grid>

            <TextBlock Text="Recurrence" Style="{StaticResource SubText}" Margin="0,0,0,4"/>
            <ComboBox x:Name="RecurrenceCombo" Margin="0,0,0,14">
                <ComboBoxItem Content="One-time"/>
                <ComboBoxItem Content="Monthly"/>
                <ComboBoxItem Content="Yearly"/>
            </ComboBox>

            <TextBlock Text="Notification Style" Style="{StaticResource SubText}" Margin="0,0,0,4"/>
            <ComboBox x:Name="NotificationStyleCombo" Margin="0,0,0,20">
                <ComboBoxItem Content="Use default"/>
                <ComboBoxItem Content="Popup (long + sound)"/>
                <ComboBoxItem Content="Banner (default)"/>
                <ComboBoxItem Content="Subtle (short + silent)"/>
            </ComboBox>

            <TextBlock x:Name="ErrorText" Foreground="{DynamicResource DangerBrush}" Margin="0,0,0,10"
                       TextWrapping="Wrap" Visibility="Collapsed"/>

            <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                <Button Content="Cancel" Style="{StaticResource GhostPillButton}" Margin="0,0,8,0" Click="Cancel_Click"/>
                <Button Content="Save" Style="{StaticResource PillButton}" Click="Save_Click"/>
            </StackPanel>
        </StackPanel>
    </ScrollViewer>
</Window>
