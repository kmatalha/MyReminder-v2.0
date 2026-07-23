<Window x:Class="RemindMe.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:conv="clr-namespace:RemindMe.Converters"
        Title="RemindMe" Height="720" Width="1080" MinWidth="860" MinHeight="560"
        Background="{DynamicResource AppBackgroundBrush}"
        FontFamily="{StaticResource AppFont}"
        WindowStartupLocation="CenterScreen"
        Closing="Window_Closing">

    <Window.Resources>
        <conv:CategoryToIconConverter x:Key="CategoryToIcon"/>
        <conv:BillStatusToBrushConverter x:Key="StatusToBrush"/>
        <conv:InverseBoolToVisibilityConverter x:Key="InverseBoolToVisibility"/>
        <conv:StringToVisibilityConverter x:Key="StringToVisibility"/>
        <BooleanToVisibilityConverter x:Key="BoolToVisibility"/>
    </Window.Resources>

    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="220"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <!-- Sidebar -->
        <Border Grid.Column="0" Background="{DynamicResource SidebarBackgroundBrush}">
            <ScrollViewer VerticalScrollBarVisibility="Auto">
            <StackPanel Margin="24,32,24,24">
                <StackPanel Orientation="Horizontal" Margin="0,0,0,36">
                    <TextBlock Text="&#xE73E;" FontFamily="{StaticResource IconFont}" FontSize="22"
                               Foreground="{DynamicResource AccentBrush}" VerticalAlignment="Center"/>
                    <TextBlock Text=" RemindMe" FontSize="19" FontWeight="Bold" Margin="8,0,0,0"
                               Foreground="{DynamicResource PrimaryTextBrush}" VerticalAlignment="Center"/>
                </StackPanel>

                <Button x:Name="AddBillButton" Content="+  Add Bill" Style="{StaticResource PillButton}"
                        HorizontalContentAlignment="Center" Margin="0,0,0,10" Click="AddBillButton_Click"/>
                <Button x:Name="HistoryButton" Content="History" Style="{StaticResource GhostPillButton}"
                        HorizontalContentAlignment="Center" Margin="0,0,0,28" Click="HistoryButton_Click"/>

                <TextBlock Text="THEME" Style="{StaticResource SubText}" Margin="4,0,0,8" FontSize="11"/>
                <Grid Margin="0,0,0,24">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>
                    <Button Grid.Column="0" Content="Light" Style="{StaticResource GhostPillButton}"
                            Margin="0,0,4,0" Click="LightTheme_Click"/>
                    <Button Grid.Column="1" Content="Dark" Style="{StaticResource GhostPillButton}"
                            Margin="4,0,0,0" Click="DarkTheme_Click"/>
                </Grid>

                <TextBlock Text="NOTIFICATION STYLE" Style="{StaticResource SubText}" Margin="4,0,0,8" FontSize="11"/>
                <ComboBox x:Name="NotificationStyleCombo" Margin="0,0,0,24"
                          SelectionChanged="NotificationStyleCombo_SelectionChanged">
                    <ComboBoxItem Content="Popup (long + sound)"/>
                    <ComboBoxItem Content="Banner (default)"/>
                    <ComboBoxItem Content="Subtle (short + silent)"/>
                </ComboBox>

                <CheckBox x:Name="StartupCheckBox" Content="Run at Windows startup" Margin="4,0,0,24"
                          Foreground="{DynamicResource PrimaryTextBrush}"
                          Checked="StartupCheckBox_Changed" Unchecked="StartupCheckBox_Changed"/>

                <TextBlock Text="ALARM SOUND" Style="{StaticResource SubText}" Margin="4,0,0,8" FontSize="11"/>
                <TextBlock x:Name="AlarmSoundNameText" Style="{StaticResource SubText}" Margin="4,0,0,8"
                           TextWrapping="Wrap" Text="Default"/>
                <Grid Margin="0,0,0,24">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>
                    <Button Grid.Column="0" Content="Choose..." Style="{StaticResource GhostPillButton}"
                            Margin="0,0,4,0" Click="ChooseAlarmSound_Click"/>
                    <Button Grid.Column="1" Content="Reset" Style="{StaticResource GhostPillButton}"
                            Margin="4,0,0,0" Click="ResetAlarmSound_Click"/>
                </Grid>

                <TextBlock Text="DATA" Style="{StaticResource SubText}" Margin="4,0,0,8" FontSize="11"/>
                <Grid Margin="0,0,0,4">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>
                    <Button Grid.Column="0" Content="Backup" Style="{StaticResource GhostPillButton}"
                            Margin="0,0,4,0" Click="BackupData_Click"/>
                    <Button Grid.Column="1" Content="Restore" Style="{StaticResource GhostPillButton}"
                            Margin="4,0,0,0" Click="RestoreData_Click"/>
                </Grid>
            </StackPanel>
            </ScrollViewer>
        </Border>

        <!-- Main content -->
        <Grid Grid.Column="1" Margin="32,28,32,24">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
            </Grid.RowDefinitions>

            <StackPanel Grid.Row="0" Margin="0,0,0,20">
                <TextBlock Text="Your Bills" Style="{StaticResource HeadingText}"/>
                <TextBlock x:Name="SubHeaderText" Style="{StaticResource SubText}" Margin="0,4,0,0"/>
            </StackPanel>

            <!-- Monthly summary strip -->
            <Border Grid.Row="1" Style="{StaticResource BillCard}" Margin="0,0,0,20">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="220"/>
                    </Grid.ColumnDefinitions>

                    <StackPanel Grid.Column="0" VerticalAlignment="Center">
                        <TextBlock Text="This Month" FontWeight="SemiBold" FontSize="15"
                                   Foreground="{DynamicResource PrimaryTextBrush}"/>
                        <TextBlock x:Name="SummaryText" Style="{StaticResource SubText}" Margin="0,4,0,0"/>
                    </StackPanel>

                    <StackPanel Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Bottom"
                                HorizontalAlignment="Right">
                        <StackPanel Width="48" Margin="8,0" VerticalAlignment="Bottom" HorizontalAlignment="Center">
                            <Border x:Name="PaidBar" Width="28" Height="10" CornerRadius="6"
                                    Background="{DynamicResource SuccessBrush}" VerticalAlignment="Bottom"/>
                            <TextBlock Text="Paid" Style="{StaticResource SubText}" HorizontalAlignment="Center" Margin="0,6,0,0"/>
                        </StackPanel>
                        <StackPanel Width="48" Margin="8,0" VerticalAlignment="Bottom" HorizontalAlignment="Center">
                            <Border x:Name="UnpaidBar" Width="28" Height="10" CornerRadius="6"
                                    Background="{DynamicResource DangerBrush}" VerticalAlignment="Bottom"/>
                            <TextBlock Text="Unpaid" Style="{StaticResource SubText}" HorizontalAlignment="Center" Margin="0,6,0,0"/>
                        </StackPanel>
                    </StackPanel>
                </Grid>
            </Border>

            <!-- Bill list -->
            <ScrollViewer Grid.Row="2" VerticalScrollBarVisibility="Auto">
                <ItemsControl x:Name="BillsList">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Border Style="{StaticResource BillCard}">
                                <Grid>
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width="Auto"/>
                                        <ColumnDefinition Width="*"/>
                                        <ColumnDefinition Width="Auto"/>
                                        <ColumnDefinition Width="Auto"/>
                                    </Grid.ColumnDefinitions>

                                    <Border Grid.Column="0" Width="44" Height="44" CornerRadius="12"
                                            Background="{DynamicResource PastelSkyBrush}" Margin="0,0,16,0">
                                        <TextBlock Text="{Binding Category, Converter={StaticResource CategoryToIcon}}"
                                                   FontFamily="{StaticResource IconFont}" FontSize="18"
                                                   Foreground="{DynamicResource AccentBrushHover}"
                                                   HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                    </Border>

                                    <StackPanel Grid.Column="1" VerticalAlignment="Center">
                                        <TextBlock Text="{Binding Name}" FontWeight="SemiBold" FontSize="15"
                                                   Foreground="{DynamicResource PrimaryTextBrush}"/>
                                        <StackPanel Orientation="Horizontal" Margin="0,4,0,0">
                                            <Ellipse Width="8" Height="8" Fill="{Binding Converter={StaticResource StatusToBrush}}"
                                                     VerticalAlignment="Center" Margin="0,0,6,0"/>
                                            <TextBlock Text="{Binding StatusText}" Style="{StaticResource SubText}"/>
                                        </StackPanel>
                                        <TextBlock Text="{Binding Description}" Style="{StaticResource SubText}"
                                                   TextWrapping="Wrap" MaxWidth="440" Margin="0,4,0,0"
                                                   Visibility="{Binding Description, Converter={StaticResource StringToVisibility}}"/>
                                    </StackPanel>

                                    <StackPanel Grid.Column="2" Orientation="Horizontal" VerticalAlignment="Center" Margin="0,0,12,0">
                                        <Button Content="Snooze" Style="{StaticResource GhostPillButton}" Padding="12,6"
                                                Tag="{Binding}" Click="Snooze_Click" Margin="0,0,8,0"/>
                                        <Button Content="Edit" Style="{StaticResource GhostPillButton}" Padding="12,6"
                                                Tag="{Binding}" Click="Edit_Click" Margin="0,0,8,0"/>
                                        <Button Content="Delete" Style="{StaticResource GhostPillButton}" Padding="12,6"
                                                Tag="{Binding}" Click="Delete_Click"/>
                                    </StackPanel>

                                    <StackPanel Grid.Column="3" Orientation="Horizontal">
                                        <Button Content="Mark Paid" Style="{StaticResource PillButton}"
                                                Tag="{Binding}" Click="MarkPaid_Click"
                                                Visibility="{Binding IsPaid, Converter={StaticResource InverseBoolToVisibility}}"/>
                                        <Button Content="Mark Unpaid" Style="{StaticResource GhostPillButton}"
                                                Tag="{Binding}" Click="MarkUnpaid_Click"
                                                Visibility="{Binding IsPaid, Converter={StaticResource BoolToVisibility}}"/>
                                    </StackPanel>
                                </Grid>
                            </Border>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </ScrollViewer>
        </Grid>
    </Grid>
</Window>
