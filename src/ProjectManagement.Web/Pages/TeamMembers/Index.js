$(function () {
    var l = abp.localization.getResource('ProjectManagement');
    var apiUrl = abp.appPath + 'api/app/team-member/team-member-dto';
    var $loading = $('#TeamMemberLoading');
    var $empty = $('#TeamMemberEmpty');
    var $error = $('#TeamMemberError');
    var $errorMessage = $('#TeamMemberErrorMessage');
    var $grid = $('#TeamMemberGrid');

    var createModal = new abp.ModalManager(
        abp.appPath + 'TeamMembers/CreateModal',
    );
    var editModal = new abp.ModalManager(abp.appPath + 'TeamMembers/EditModal');

    createModal.onResult(function () {
        loadTeamMembers();
    });

    editModal.onResult(function () {
        loadTeamMembers();
    });

    $('#NewTeamMemberButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    $('#RetryTeamMembersButton').on('click', function (e) {
        e.preventDefault();
        loadTeamMembers();
    });

    loadTeamMembers();

    function loadTeamMembers() {
        setState('loading');

        abp.ajax({
            type: 'GET',
            url: apiUrl,
            data: {
                SkipCount: 0,
                MaxResultCount: 1000,
                Sorting: 'name',
            },
        })
            .done(function (response) {
                var items = response && response.items ? response.items : [];

                renderCards(items);
                setState(items.length ? 'content' : 'empty');
            })
            .fail(function (error) {
                var message =
                    error &&
                    error.responseJSON &&
                    error.responseJSON.error &&
                    error.responseJSON.error.message
                        ? error.responseJSON.error.message
                        : l('CouldNotLoadTeamMembers');

                $errorMessage.text(message);
                setState('error');
            });
    }

    function setState(state) {
        $loading.toggleClass('d-none', state !== 'loading');
        $empty.toggleClass('d-none', state !== 'empty');
        $error.toggleClass('d-none', state !== 'error');
        $grid.toggleClass('d-none', state !== 'content');
    }

    function renderCards(items) {
        $grid.empty();

        items.forEach(function (item) {
            $grid.append(buildCard(item));
        });
    }

    function buildCard(item) {
        var initials = getInitials(item.name || '');
        var weeklyCapacity = normalizeNumber(item.weeklyCapacity);
        var currentCapacity = normalizeNumber(item.currentCapacity);
        var freeCapacity = Math.max(weeklyCapacity - currentCapacity, 0);
        var usedPercent =
            weeklyCapacity > 0
                ? Math.min((currentCapacity / weeklyCapacity) * 100, 100)
                : 0;
        var avatarColor = pickAvatarColor(item.name || item.email || '');

        var $col = $('<div/>', { class: 'col-12 col-md-6 col-xl-4' });
        var $card = $('<div/>', { class: 'team-member-card' });
        var $header = $('<div/>', { class: 'team-member-card__header' });
        var $avatar = $('<div/>', { class: 'team-member-avatar' })
            .css('background', avatarColor)
            .text(initials);
        var $identity = $('<div/>', { class: 'team-member-identity' });
        var $name = $('<div/>', { class: 'team-member-name' }).text(
            item.name || '-',
        );
        var $role = $('<div/>', { class: 'team-member-role' }).text(
            item.role || '-',
        );
        var $actions = $('<div/>', { class: 'team-member-actions' });

        var canEdit = abp.auth.isGranted('ProjectManagement.TeamMembers.Edit');
        var canDelete = abp.auth.isGranted(
            'ProjectManagement.TeamMembers.Delete',
        );
        var canView = canEdit || canDelete;

        if (canEdit) {
            $actions.append(
                buildIconButton('fa fa-pen', l('Edit'), function () {
                    editModal.open({ id: item.id });
                }),
            );
        }

        if (canDelete) {
            $actions.append(
                buildIconButton(
                    'fa fa-trash',
                    l('Delete'),
                    function () {
                        abp.message.confirm(
                            l('AreYouSureToDelete'),
                            l('AreYouSure'),
                            function (confirmed) {
                                if (!confirmed) {
                                    return;
                                }

                                abp.ajax({
                                    type: 'DELETE',
                                    url:
                                        abp.appPath +
                                        'api/app/team-member/' +
                                        item.id +
                                        '/team-member',
                                }).done(function () {
                                    abp.notify.success(
                                        l('DeletedSuccessfully'),
                                    );
                                    loadTeamMembers();
                                });
                            },
                        );
                    },
                    'text-danger',
                ),
            );
        }

        $identity.append($name, $role);
        $header.append($avatar, $identity, $actions);

        var $capacityRow = $('<div/>', { class: 'team-member-capacity-row' });
        var $capacityLabel = $('<span/>').text(l('WeeklyCapacity'));
        var $capacityValue = $('<span/>', {
            class: 'team-member-capacity-value',
        }).text(currentCapacity + 'h / ' + weeklyCapacity + 'h');
        var $capacityFree = $('<span/>', {
            class: 'team-member-capacity-free',
        }).text(freeCapacity + 'h ' + l('Free'));

        var $progressTrack = $('<div/>', { class: 'team-member-progress' });
        var $progressBar = $('<div/>', {
            class: 'team-member-progress__bar',
        }).css('width', usedPercent + '%');

        $progressTrack.append($progressBar);

        var $footer = $('<div/>', { class: 'team-member-card__footer' });
        var $email = $('<div/>', { class: 'team-member-email' });
        $email.append($('<i/>', { class: 'fa fa-envelope' }));
        $email.append($('<span/>').text(item.email || '-'));

        var $detailsButton = $('<button/>', {
            type: 'button',
            class: 'btn btn-primary team-member-details-button',
        })
            .text(l('Details'))
            .on('click', function () {
                return;
            });

        $capacityRow.append(
            $capacityLabel,
            $('<div/>', { class: 'team-member-capacity-summary' }).append(
                $capacityValue,
                $capacityFree,
            ),
        );
        $footer.append($email, $detailsButton);

        if (!canView) {
            $actions.addClass('team-member-actions--hidden');
        }

        $card.append($header, $capacityRow, $progressTrack, $footer);
        $col.append($card);

        return $col;
    }

    function buildIconButton(iconClass, title, onClick, buttonClass) {
        return $('<button/>', {
            type: 'button',
            class: 'team-member-icon-button ' + (buttonClass || ''),
            title: title,
            'aria-label': title,
        })
            .append($('<i/>', { class: iconClass }))
            .on('click', onClick);
    }

    function normalizeNumber(value) {
        var parsed = parseInt(value, 10);
        return isNaN(parsed) ? 0 : parsed;
    }

    function getInitials(name) {
        var parts = name.trim().split(/\s+/).filter(Boolean);
        if (!parts.length) {
            return 'TM';
        }

        return parts
            .slice(0, 2)
            .map(function (part) {
                return part.charAt(0).toUpperCase();
            })
            .join('');
    }

    function pickAvatarColor(seed) {
        var palette = ['#ffe2ec', '#e8f1ff', '#e7f8ef', '#f8ecff', '#fff1dd'];
        var index = Math.abs(hashCode(seed)) % palette.length;
        return palette[index];
    }

    function hashCode(text) {
        var hash = 0;
        for (var i = 0; i < text.length; i++) {
            hash = (hash << 5) - hash + text.charCodeAt(i);
            hash |= 0;
        }
        return hash;
    }
});
